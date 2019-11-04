// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using GameTheory.GameTree;
    using GameTheory.GameTree.Caches;

    public class MonteCarloTreeSearchPlayer<TMove> : MaximizingPlayerBase<TMove, ResultScore<byte>>
        where TMove : IMove
    {
        private readonly TimeSpan thinkTime;

        public MonteCarloTreeSearchPlayer(PlayerToken playerToken, int thinkSeconds = 5)
            : base(playerToken, new ResultScoringMetric<TMove, byte>(ScoringMetric.Null<PlayerState<TMove>>()))
        {
            this.thinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds));
        }

        protected override Mainline<TMove, ResultScore<byte>> GetMove(List<IGameState<TMove>> states, bool ponder, CancellationToken cancel)
        {
            var nodes = states.Select(this.gameTree.GetOrAdd).ToList();

            var sw = Stopwatch.StartNew();
            while (true)
            {
                this.Walk(nodes.Pick());

                if (cancel.IsCancellationRequested ||
                    sw.Elapsed > this.thinkTime ||
                    nodes.All(n => n.Mainline?.FullyDetermined ?? false))
                {
                    break;
                }
            }

            var mainline = this.CombineMainlines(nodes.Select(n => n.Mainline).Where(m => m != null).ToList());

            if (!ponder)
            {
                this.SendMessage(mainline);
                this.SendMessage("Time taken: ", sw.ElapsedMilliseconds, " ms");
            }

            return mainline;
        }

        /// <inheritdoc />
        protected override IGameStateCache<TMove, ResultScore<byte>> MakeCache() => new DictionaryCache<TMove, ResultScore<byte>>();

        private Mainline<TMove, ResultScore<byte>> GetMaximizingMoves(PlayerToken player, IList<Mainline<TMove, ResultScore<byte>>> moveScores)
        {
            var maxLead = default(Maybe<ResultScore<byte>>);
            var fullyDetermined = true;
            var unvisited = 0;
            var maxMainlines = new List<Mainline<TMove, ResultScore<byte>>>();

            for (var m = 0; m < moveScores.Count; m++)
            {
                var mainline = moveScores[m];
                if (mainline == null)
                {
                    unvisited++;
                    fullyDetermined = false;
                }
                else
                {
                    fullyDetermined &= mainline.FullyDetermined;

                    if (!maxLead.HasValue)
                    {
                        maxLead = this.GetLead(mainline, player);
                        maxMainlines.Add(mainline);
                    }
                    else
                    {
                        var lead = this.GetLead(mainline, player);
                        var comp = this.scoringMetric.Compare(lead, maxLead.Value);
                        if (comp >= 0)
                        {
                            if (comp > 0)
                            {
                                maxLead = lead;
                                maxMainlines.Clear();
                            }

                            maxMainlines.Add(mainline);
                        }
                    }
                }
            }

            if (unvisited == moveScores.Count)
            {
                throw new InvalidOperationException();
            }

            var sourceMainline = maxMainlines.Pick();
            var scores = sourceMainline.Scores;
            if (unvisited > 0)
            {
                var players = sourceMainline.GameState.Players;
                var winHorizonPly = 1000; // TODO: Max? Max + 1?
                var combinedScore = new Dictionary<PlayerToken, ResultScore<byte>>();
                var playerScore = new Weighted<ResultScore<byte>>[2];
                foreach (var p in players)
                {
                    playerScore[0] = Weighted.Create(scores[p], moveScores.Count - unvisited);
                    playerScore[1] = Weighted.Create(new ResultScore<byte>(Result.None, winHorizonPly, 1, 0), unvisited);

                    combinedScore[p] = this.scoringMetric.Combine(playerScore);
                }

                scores = combinedScore;
            }

            var maxMoves = maxMainlines.SelectMany(m => m.Strategies.Peek()).ToImmutableArray();
            var depth = fullyDetermined ? moveScores.Max(m => m.Depth) : moveScores.Where(m => !(m?.FullyDetermined ?? false)).Min(m => m?.Depth ?? 0);
            return new Mainline<TMove, ResultScore<byte>>(scores, sourceMainline.GameState, sourceMainline.PlayerToken, sourceMainline.Strategies.Pop().Push(maxMoves), depth, fullyDetermined);
        }

        private void Maximize(StateNode<TMove, ResultScore<byte>> node)
        {
            var allMoves = node.Moves;
            var mainlines = new List<Mainline<TMove, ResultScore<byte>>>(allMoves.Length);
            for (var m = 0; m < allMoves.Length; m++)
            {
                var move = allMoves[m];
                var moveNode = node[move];

                // Expectimax.
                var outcomes = moveNode.Outcomes;
                var weightedOutcomes = new List<Weighted<Mainline<TMove, ResultScore<byte>>>>();
                foreach (var outcome in outcomes)
                {
                    var mainline = outcome.Value.Mainline;
                    if (mainline != null)
                    {
                        var strategy = ImmutableArray.Create<IWeighted<TMove>>(Weighted.Create(move, 1));

                        mainline = mainline.Extend(move.PlayerToken, strategy, this.scoreExtender);
                    }

                    weightedOutcomes.Add(Weighted.Create(mainline, outcome.Weight));
                }

                mainlines.Add(this.CombineOutcomes(node.State, weightedOutcomes));
            }

            var playerLeads = (from pm in allMoves.Select((m, i) => new { Move = m, Mainline = mainlines[i] })
                               group pm.Mainline by pm.Move.PlayerToken into g
                               let playerToken = g.Key
                               let mainline = this.GetMaximizingMoves(playerToken, g.ToList())
                               let lead = this.GetLead(mainline, playerToken)
                               select new
                               {
                                   Mainline = mainline,
                                   PlayerToken = playerToken,
                                   Lead = lead,
                               }).ToList();

            // TODO: Apply simultaneous move rules.
            // TODO: Only fully determined if all player's scores are fully determined.
            node.Mainline = playerLeads.Single().Mainline;
        }

        private void Playout(StateNode<TMove, ResultScore<byte>> node)
        {
            if (node.Mainline?.FullyDetermined ?? false)
            {
                return;
            }

            var state = node.State;
            IList<TMove> moves = node.Moves;
            if (moves.Count == 0)
            {
                node.Mainline = new Mainline<TMove, ResultScore<byte>>(this.scoringMetric.Score(state), state, null, ImmutableStack<IReadOnlyList<IWeighted<TMove>>>.Empty, depth: 0, fullyDetermined: true);
                return;
            }

            var move = moves.Pick();
            var moveNode = node[move];
            var outcome = moveNode.Outcomes.Pick();

            this.Playout(outcome);

            this.Maximize(node);
        }

        private void Walk(StateNode<TMove, ResultScore<byte>> node)
        {
            if (node.Mainline == null)
            {
                this.Playout(node);
            }
            else if (node.Mainline.FullyDetermined)
            {
                return;
            }
            else
            {
                IList<TMove> moves = node.Moves;
                if (moves.Count == 0)
                {
                    this.Playout(node);
                    return;
                }

                TMove move;
                MoveNode<TMove, ResultScore<byte>> moveNode;
                StateNode<TMove, ResultScore<byte>> outcome;

                if (!node.Mainline.Strategies.IsEmpty && GameTheory.Random.Instance.NextDouble() < 0.9)
                {
                    move = node.Mainline.Strategies.Peek().Pick();
                    moveNode = node[move];
                    outcome = moveNode.Outcomes.Pick();
                }
                else
                {
                    move = moves.Pick();
                    moveNode = node[move];
                    outcome = moveNode.Outcomes.Pick();
                }

                if (outcome.Mainline?.FullyDetermined ?? false)
                {
                    var remainingOutcomes = moveNode.Outcomes.Where(o => !(o.Value.Mainline?.FullyDetermined ?? false)).ToList();
                    if (remainingOutcomes.Count > 0)
                    {
                        outcome = remainingOutcomes.Pick();
                    }
                    else
                    {
                        var remainingMoves = moves.Where(m => node[m].Outcomes.Any(o => !(o.Value.Mainline?.FullyDetermined ?? false))).ToList();
                        if (remainingMoves.Count > 0)
                        {
                            move = remainingMoves.Pick();
                            moveNode = node[move];
                            remainingOutcomes = moveNode.Outcomes.Where(o => !(o.Value.Mainline?.FullyDetermined ?? false)).ToList();
                            outcome = remainingOutcomes.Pick();
                        }
                        else
                        {
                            this.Maximize(node);
                            return;
                        }
                    }
                }

                this.Walk(outcome);

                this.Maximize(node);
            }
        }
    }
}
