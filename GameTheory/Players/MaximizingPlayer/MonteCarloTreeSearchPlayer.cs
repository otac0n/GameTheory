// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MonteCarloTreeSearchPlayer<TMove> : IPlayer<TMove>
        where TMove : IMove
    {
        private readonly GameTree<TMove, ResultScore<byte>> gameTree;
        private readonly ResultScoringMetric<TMove, byte> scoringMetric;
        private readonly TimeSpan thinkTime;

        public MonteCarloTreeSearchPlayer(PlayerToken playerToken, int thinkSeconds = 5)
        {
            this.PlayerToken = playerToken;
            this.thinkTime = TimeSpan.FromSeconds(thinkSeconds);
            this.gameTree = new GameTree<TMove, ResultScore<byte>>(this.MakeCache());
            this.scoringMetric = new ResultScoringMetric<TMove, byte>(ScoringMetric.Null<PlayerState<TMove>>());
        }

        /// <inheritdoc />
        public event EventHandler<MessageSentEventArgs> MessageSent;

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <summary>
        /// Gets the maximum number of states to sample from all possible initial states.
        /// </summary>
        protected virtual int InitialSamples => 100;

        /// <inheritdoc />
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel)
        {
            await Task.Yield();

            var sw = Stopwatch.StartNew();
            var moves = state.GetAvailableMoves();
            var players = moves.Select(m => m.PlayerToken).ToImmutableHashSet();
            if (!players.Contains(this.PlayerToken))
            {
                return default(Maybe<TMove>);
            }
            else
            {
                if (players.Count == 1 && moves.Count == 1)
                {
                    // If we are forced to move, don't spend any time determining the correct move.
                    return moves.Single();
                }
            }

            var states = state.GetView(this.PlayerToken, this.InitialSamples).ToList();
            var nodes = states.Select(this.gameTree.GetOrAdd).ToList();

            for (var i = 0; ; i++)
            {
                this.Walk(nodes.Pick());

                if (cancel.IsCancellationRequested ||
                    sw.Elapsed > this.thinkTime ||
                    nodes.All(n => n.Mainline?.FullyDetermined ?? false))
                {
                    break;
                }
            }

            // TODO: Combine outcomes.
            var mainline = nodes.Pick().Mainline;

            this.MessageSent?.Invoke(this, new MessageSentEventArgs(mainline));
            var chosen = mainline.Strategies.Peek().Pick();
            sw.Stop();
            this.MessageSent?.Invoke(this, new MessageSentEventArgs("Time taken: ", sw.ElapsedMilliseconds, " ms"));
            this.gameTree.Trim();
            return chosen;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <summary>
        /// Gets the lead of a specific player over the field.  In order to support games of 3 or more players, this class tries to maximize the lead rather than the actual score value.
        /// In cooperative games, override this to ensure that coalitions of players cooperate.
        /// </summary>
        /// <param name="score">Contains the scores for the players in the specified game state.</param>
        /// <param name="state">The <see cref="IGameState{TMove}"/> that was scored.</param>
        /// <param name="player">The player whose lead should be retrieved.</param>
        /// <returns>The player's lead, as a score.</returns>
        protected virtual ResultScore<byte> GetLead(IDictionary<PlayerToken, ResultScore<byte>> score, IGameState<TMove> state, PlayerToken player)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            if (state.Players.Count == 1)
            {
                return score[player];
            }
            else
            {
                var any = false;
                var max = default(ResultScore<byte>);
                foreach (var other in state.Players)
                {
                    if (other != player)
                    {
                        var value = score[other];
                        if (!any || this.scoringMetric.Compare(value, max) > 0)
                        {
                            max = value;
                            any = true;
                        }
                    }
                }

                return this.scoringMetric.Difference(
                    score[player],
                    max);
            }
        }

        /// <summary>
        /// Create the transposition table cache.
        /// </summary>
        /// <returns>The cache object to use as a transposition table.</returns>
        protected virtual ICache<TMove, ResultScore<byte>> MakeCache() => new Caches.DictionaryCache<TMove, ResultScore<byte>>();

        private Mainline<TMove, ResultScore<byte>> CombineOutcomes(IList<Weighted<Mainline<TMove, ResultScore<byte>>>> mainlines)
        {
            var firstMainline = mainlines[0].Value;
            if (mainlines.Count == 1)
            {
                return firstMainline;
            }

            var maxWeight = double.NegativeInfinity;
            var maxPlayerToken = (PlayerToken)null;
            Mainline<TMove, ResultScore<byte>> maxMainline = null;
            var fullyDetermined = true;

            for (var i = 0; i < mainlines.Count; i++)
            {
                var weightedMainline = mainlines[i];
                var weight = weightedMainline.Weight;
                var mainline = weightedMainline.Value;
                var score = mainline.Scores;
                var playerToken = mainline.PlayerToken;

                var playerCompare = (playerToken == this.PlayerToken).CompareTo(maxPlayerToken == this.PlayerToken);
                var weightCompare = weight.CompareTo(maxWeight);
                if (playerCompare > 0 || (playerCompare == 0 && weightCompare > 0))
                {
                    maxWeight = weight;
                    maxMainline = mainline;
                    maxPlayerToken = playerToken;
                }

                fullyDetermined &= mainline.FullyDetermined;
            }

            var combinedScore = new Dictionary<PlayerToken, ResultScore<byte>>();
            var playerScore = new Weighted<ResultScore<byte>>[mainlines.Count];
            foreach (var player in firstMainline.GameState.Players)
            {
                for (var i = 0; i < mainlines.Count; i++)
                {
                    playerScore[i] = Weighted.Create(mainlines[i].Value.Scores[player], mainlines[i].Weight);
                }

                combinedScore[player] = this.scoringMetric.Combine(playerScore);
            }

            var depth = fullyDetermined ? mainlines.Max(m => m.Value.Depth) : mainlines.Where(m => !m.Value.FullyDetermined).Min(m => m.Value.Depth);
            return new Mainline<TMove, ResultScore<byte>>(combinedScore, maxMainline.GameState, maxMainline.PlayerToken, maxMainline.Strategies, depth, fullyDetermined);
        }

        private ResultScore<byte> GetLead(Mainline<TMove, ResultScore<byte>> mainline, PlayerToken player) => this.GetLead(mainline.Scores, mainline.GameState, player);

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
                        var scores = new Dictionary<PlayerToken, ResultScore<byte>>();
                        foreach (var player in mainline.GameState.Players)
                        {
                            scores.Add(player, this.scoringMetric.Extend(mainline.Scores[player]));
                        }

                        mainline = new Mainline<TMove, ResultScore<byte>>(scores, mainline.GameState, move.PlayerToken, mainline.Strategies.Push(ImmutableArray.Create<IWeighted<TMove>>(Weighted.Create(move, 1))), mainline.Depth + 1, mainline.FullyDetermined);
                    }

                    weightedOutcomes.Add(Weighted.Create(mainline, outcome.Weight));
                }

                mainlines.Add(this.CombineOutcomes(weightedOutcomes));
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

        private Mainline<TMove, ResultScore<byte>> Maximize(PlayerToken player, Mainline<TMove, ResultScore<byte>> a, Mainline<TMove, ResultScore<byte>> b)
        {
            var leadA = this.GetLead(a, player);
            var leadB = this.GetLead(b, player);

            bool fullyDetermined;
            int depth;

            if (a.FullyDetermined)
            {
                if (b.FullyDetermined)
                {
                    fullyDetermined = true;
                    depth = Math.Max(a.Depth, b.Depth);
                }
                else
                {
                    fullyDetermined = false;
                    depth = b.Depth;
                }
            }
            else if (b.FullyDetermined)
            {
                fullyDetermined = false;
                depth = a.Depth;
            }
            else
            {
                fullyDetermined = false;
                depth = Math.Min(a.Depth, b.Depth);
            }

            var comp = this.scoringMetric.Compare(leadA, leadB);
            if (comp == 0)
            {
                var strategies = a.Strategies.Pop().Push(a.Strategies.Peek().Concat(b.Strategies.Peek()).ToList());
                return new Mainline<TMove, ResultScore<byte>>(a.Scores, a.GameState, a.PlayerToken, strategies, depth, fullyDetermined);
            }
            else if (comp > 0)
            {
                if (a.FullyDetermined == fullyDetermined && a.Depth == depth)
                {
                    return a;
                }

                return new Mainline<TMove, ResultScore<byte>>(a.Scores, a.GameState, a.PlayerToken, a.Strategies, depth, fullyDetermined);
            }
            else
            {
                if (b.FullyDetermined == fullyDetermined && b.Depth == depth)
                {
                    return b;
                }

                return new Mainline<TMove, ResultScore<byte>>(b.Scores, b.GameState, b.PlayerToken, b.Strategies, depth, fullyDetermined);
            }
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
            var outcome = moveNode.Outcomes.Pick().Value;

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
                    outcome = moveNode.Outcomes.Pick().Value;
                }
                else
                {
                    move = moves.Pick();
                    moveNode = node[move];
                    outcome = moveNode.Outcomes.Pick().Value;
                }

                if (outcome.Mainline?.FullyDetermined ?? false)
                {
                    var remainingOutcomes = moveNode.Outcomes.Where(o => !(o.Value.Mainline?.FullyDetermined ?? false)).ToList();
                    if (remainingOutcomes.Count > 0)
                    {
                        outcome = remainingOutcomes.Pick().Value;
                    }
                    else
                    {
                        var remainingMoves = moves.Where(m => node[m].Outcomes.Any(o => !(o.Value.Mainline?.FullyDetermined ?? false))).ToList();
                        if (remainingMoves.Count > 0)
                        {
                            move = remainingMoves.Pick();
                            moveNode = node[move];
                            remainingOutcomes = moveNode.Outcomes.Where(o => !(o.Value.Mainline?.FullyDetermined ?? false)).ToList();
                            outcome = remainingOutcomes.Pick().Value;
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
