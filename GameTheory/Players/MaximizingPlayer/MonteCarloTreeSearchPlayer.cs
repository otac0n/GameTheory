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

    public class MonteCarloTreeSearchPlayer<TMove> : MaximizingPlayerBase<TMove, WinCount>
            where TMove : IMove
    {
        private readonly TimeSpan thinkTime;

        public MonteCarloTreeSearchPlayer(PlayerToken playerToken, int thinkSeconds = 5)
            : base(playerToken, new WinCountScoringMetric<TMove>())
        {
            this.thinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds));
        }

        /// <inheritdoc/>
        protected override Mainline<TMove, WinCount> GetMove(List<IGameState<TMove>> states, bool ponder, CancellationToken cancel)
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
        protected override IGameStateCache<TMove, WinCount> MakeCache() => new DictionaryCache<TMove, WinCount>();

        private Mainline<TMove, WinCount> GetMaximizingMoves(PlayerToken player, IList<Mainline<TMove, WinCount>> moveScores)
        {
            var fullyDetermined = true;
            IDictionary<PlayerToken, WinCount> totalScores = null;
            var scoresAllocated = false;

            for (var m = 0; m < moveScores.Count; m++)
            {
                var mainline = moveScores[m];
                if (mainline != null)
                {
                    if (totalScores == null)
                    {
                        totalScores = mainline.Scores;
                    }
                    else
                    {
                        if (!scoresAllocated)
                        {
                            totalScores = new Dictionary<PlayerToken, WinCount>(totalScores);
                            scoresAllocated = true;
                        }

                        var scores = mainline.Scores;
                        foreach (var p in scores.Keys)
                        {
                            var score = scores[p];
                            totalScores[p] = totalScores.TryGetValue(p, out var totalScore)
                                ? new WinCount(totalScore.Wins + score.Wins, totalScore.Simulations + score.Simulations)
                                : score;
                        }
                    }

                    if (fullyDetermined)
                    {
                        fullyDetermined = mainline.FullyDetermined;
                    }
                }
                else
                {
                    fullyDetermined = false;
                }
            }

            Debug.Assert(totalScores != null, "At least one move must be scored.");

            var logOfTotalSimulations = Math.Log(totalScores[player].Simulations);
            double? maxScore = default;
            var maxMainlines = new List<Mainline<TMove, WinCount>>();

            for (var m = 0; m < moveScores.Count; m++)
            {
                var mainline = moveScores[m];
                if (mainline != null)
                {
                    var score = mainline.Scores[player];
                    var moveScore = score.Ratio;
                    if (!maxScore.HasValue)
                    {
                        maxScore = moveScore;
                        maxMainlines.Add(mainline);
                    }
                    else
                    {
                        var comp = moveScore.CompareTo(maxScore.Value);
                        if (comp >= 0)
                        {
                            if (comp > 0)
                            {
                                maxScore = moveScore;
                                maxMainlines.Clear();
                            }

                            maxMainlines.Add(mainline);
                        }
                    }
                }
            }

            var sourceMainline = maxMainlines.Pick();
            var maxMoves = maxMainlines.SelectMany(m => m.Strategies.Peek()).ToImmutableArray();
            var depth = fullyDetermined ? moveScores.Max(m => m.Depth) : moveScores.Where(m => !(m?.FullyDetermined ?? false)).Min(m => m?.Depth ?? 0);
            return new Mainline<TMove, WinCount>(totalScores, sourceMainline.GameState, sourceMainline.PlayerToken, sourceMainline.Strategies.Pop().Push(maxMoves), depth, fullyDetermined);
        }

        private void Maximize(StateNode<TMove, WinCount> node)
        {
            var allMoves = node.Moves;
            var mainlines = new Mainline<TMove, WinCount>[allMoves.Length];
            for (var m = 0; m < allMoves.Length; m++)
            {
                var move = allMoves[m];
                var moveNode = node[move];

                // Expectimax.
                var outcomes = moveNode.Outcomes;
                var weightedOutcomes = new List<Weighted<Mainline<TMove, WinCount>>>();
                foreach (var outcome in outcomes)
                {
                    var mainline = outcome.Value.Mainline;
                    if (mainline != null)
                    {
                        var strategy = ImmutableArray.Create<IWeighted<TMove>>(Weighted.Create(move, 1));

                        mainline = mainline.Extend(move.PlayerToken, strategy, this.scoreExtender);

                        weightedOutcomes.Add(Weighted.Create(mainline, outcome.Weight));
                    }
                }

                if (weightedOutcomes.Count > 0)
                {
                    mainlines[m] = this.CombineOutcomes(node.State, weightedOutcomes);
                }
            }

            var playerLeads = (from pm in allMoves.Select((m, i) => new { Move = m, Mainline = mainlines[i] })
                               group pm.Mainline by pm.Move.PlayerToken into g
                               let playerToken = g.Key
                               let mainline = this.GetMaximizingMoves(playerToken, g.ToList())
                               select new
                               {
                                   Mainline = mainline,
                                   PlayerToken = playerToken,
                               }).ToList();

            // TODO: Apply simultaneous move rules.
            // TODO: Only fully determined if all player's scores are fully determined.
            node.Mainline = playerLeads.Single().Mainline;
        }

        private void Playout(StateNode<TMove, WinCount> node)
        {
            if (node.Mainline?.FullyDetermined ?? false)
            {
                return;
            }

            var state = node.State;
            IList<TMove> moves = node.Moves;
            if (moves.Count == 0)
            {
                node.Mainline = new Mainline<TMove, WinCount>(this.scoringMetric.Score(state), state, null, ImmutableStack<IReadOnlyList<IWeighted<TMove>>>.Empty, depth: 0, fullyDetermined: true);
                return;
            }

            var move = moves.Pick();
            var moveNode = node[move];
            var outcome = moveNode.Outcomes.Pick();

            this.Playout(outcome);

            this.Maximize(node);
        }

        private void Walk(StateNode<TMove, WinCount> node)
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

                var totalSimulations = moves.Sum(m => node[m].Score.Simulations);
                var logOfTotalSimulations = Math.Log(totalSimulations);

                var selection = moves.AllMaxBy(m =>
                {
                    var moveNode = node[m];
                    if (moveNode.Outcomes.All(o => o.Value.Mainline?.FullyDetermined == true))
                    {
                        return double.NegativeInfinity;
                    }

                    var score = node[m].Score;
                    if (score.Simulations == 0)
                    {
                        return double.PositiveInfinity;
                    }

                    return score.Ratio + Math.Sqrt(2 * logOfTotalSimulations / score.Simulations);
                }).Pick();

                var selectionNode = node[selection];
                var outcome = selectionNode.Outcomes.Pick();

                if (outcome.Mainline?.FullyDetermined == true)
                {
                    var remainingOutcomes = selectionNode.Outcomes.Where(o => o.Value.Mainline?.FullyDetermined != true).ToList();
                    if (remainingOutcomes.Count > 0)
                    {
                        outcome = remainingOutcomes.Pick();
                    }
                }

                this.Walk(outcome);

                this.Maximize(node);
            }
        }
    }
}
