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
    using GameTheory.GameTree;
    using GameTheory.GameTree.Caches;

    /// <summary>
    /// Implements a player that maximizes a scoring function to some move depth (ply).
    /// </summary>
    /// <typeparam name="TMove">The type of move that the player supports.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    /// <remarks>
    /// The maximizing player is a generalization of the min-max concept to games that have more than two players and that may allow moves by more than one player at a time.
    /// </remarks>
    public abstract class MaximizingPlayer<TMove, TScore> : MaximizingPlayerBase<TMove, TScore>
        where TMove : IMove
    {
        private int cacheHits;
        private int cacheMisses;
        private int nodesSearched;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximizingPlayer{TMove, TScore}"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="scoringMetric">The scoring metric to use.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        protected MaximizingPlayer(PlayerToken playerToken, IGameStateScoringMetric<TMove, TScore> scoringMetric, int minPly)
            : base(playerToken, scoringMetric)
        {
            this.MinPly = minPly > -1 ? minPly : throw new ArgumentOutOfRangeException(nameof(minPly));
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MaximizingPlayer{TMove, TScore}"/> class.
        /// </summary>
        ~MaximizingPlayer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the minimum number of ply to think ahead.
        /// </summary>
        protected int MinPly { get; }

        /// <inheritdoc />
        protected override Mainline<TMove, TScore> GetMove(List<IGameState<TMove>> states, bool ponder, CancellationToken cancel)
        {
            var sw = Stopwatch.StartNew();

            var ply = this.MinPly;
            if (ponder)
            {
                ply += 1;
            }

            this.nodesSearched = 0;
            this.cacheHits = 0;
            this.cacheMisses = 0;

            Mainline<TMove, TScore> mainline;
            if (states.Count == 1)
            {
                mainline = this.GetMove(states.Single(), ply, cancel);
            }
            else
            {
                mainline = this.GetMove(states, ply, cancel);
            }

            if (!ponder)
            {
                this.SendMessage(mainline);
                this.SendMessage("Time taken: ", sw.ElapsedMilliseconds, " ms, Nodes searched: ", this.nodesSearched, " (", (this.nodesSearched / sw.Elapsed.TotalMilliseconds).ToString("F2"), " kn/s), Cache hits: ", ((double)this.cacheHits / this.nodesSearched).ToString("P"), ", Cache misses: ", ((double)this.cacheMisses / this.nodesSearched).ToString("P"));
            }

            return mainline;
        }

        /// <inheritdoc />
        protected override IGameStateCache<TMove, TScore> MakeCache() => new SplayTreeCache<TMove, TScore>();

        private Mainline<TMove, TScore> GetMove(IList<IGameState<TMove>> states, int ply, CancellationToken cancel)
        {
            var mainlines = new List<Mainline<TMove, TScore>>(states.Count);
            foreach (var state in states)
            {
                var mainline = this.GetMove(state, ply - 1, cancel);
                mainlines.Add(mainline);
            }

            return this.CombineMainlines(mainlines);
        }

        private Mainline<TMove, TScore> GetMove(IGameState<TMove> state, int ply, CancellationToken cancel)
        {
            // Iterative deepening
            var node = this.gameTree.GetOrAdd(state);
            var mainline = node.Mainline;
            for (var i = 1; i <= ply && !(mainline != null && (mainline.FullyDetermined || mainline.Depth >= ply)); i++)
            {
                mainline = this.GetMove(node, i, ImmutableDictionary<PlayerToken, IDictionary<PlayerToken, TScore>>.Empty, cancel);
            }

            return mainline;
        }

        private Mainline<TMove, TScore> GetMove(StateNode<TMove, TScore> node, int ply, ImmutableDictionary<PlayerToken, IDictionary<PlayerToken, TScore>> alphaBetaScore, CancellationToken cancel)
        {
            Interlocked.Increment(ref this.nodesSearched);
            var cached = node.Mainline;
            if (cached != null)
            {
                if (cached.Depth >= ply || cached.FullyDetermined)
                {
                    Interlocked.Increment(ref this.cacheHits);
                    return cached;
                }
            }
            else
            {
                Interlocked.Increment(ref this.cacheMisses);
            }

            return node.Mainline = this.GetMoveImpl(node, ply, alphaBetaScore, cancel);
        }

        private Mainline<TMove, TScore> GetMoveImpl(StateNode<TMove, TScore> node, int ply, ImmutableDictionary<PlayerToken, IDictionary<PlayerToken, TScore>> alphaBetaScore, CancellationToken cancel)
        {
            var state = node.State;

            if (ply == 0)
            {
                return new Mainline<TMove, TScore>(this.scoringMetric.Score(state), state, null, ImmutableStack<IReadOnlyList<IWeighted<TMove>>>.Empty, depth: 0, fullyDetermined: false);
            }

            var allMoves = node.Moves;
            var players = allMoves.Select(m => m.PlayerToken).ToImmutableHashSet();

            // For Alpha-beta pruning.
            PlayerToken singlePlayer, otherPlayer;

            // If only one player can move, they must choose a move.
            // If more than one player can move, then we assume that players will play moves that improve their position as well as moves that would result in a smaller loss than the best move of an opponent.
            // If this is a stalemate (or there are no moves), we return no move and score the current position (recurse with ply 0)
            switch (players.Count)
            {
                case 0:
                    return new Mainline<TMove, TScore>(this.scoringMetric.Score(state), state, null, ImmutableStack<IReadOnlyList<IWeighted<TMove>>>.Empty, depth: 0, fullyDetermined: true);

                case 1:
                    singlePlayer = players.Single();
                    otherPlayer = state.Players.Count == 2 ? state.GetNextPlayer(singlePlayer) : null;
                    break;

                default:
                    singlePlayer = null;
                    otherPlayer = null;
                    break;
            }

            if (otherPlayer != null)
            {
                Array.Sort(allMoves, (m1, m2) => this.scoringMetric.Compare(node[m1].Score, node[m2].Score));
            }

            var mainlines = new List<Mainline<TMove, TScore>>(allMoves.Length);
            for (var m = 0; m < allMoves.Length; m++)
            {
                var move = allMoves[m];
                var moveNode = node[move];

                // Expectimax.
                var outcomes = moveNode.Outcomes;
                var weightedOutcomes = new List<Weighted<Mainline<TMove, TScore>>>();
                foreach (var outcome in outcomes)
                {
                    var mainline = this.GetMove(outcome.Value, ply - 1, alphaBetaScore, cancel);
                    var strategy = ImmutableArray.Create<IWeighted<TMove>>(Weighted.Create(move, 1));
                    var newMainline = mainline.Extend(move.PlayerToken, strategy, this.scoreExtender);
                    weightedOutcomes.Add(Weighted.Create(newMainline, outcome.Weight));
                }

                var combined = this.CombineOutcomes(state, weightedOutcomes);
                mainlines.Add(combined);

                if (otherPlayer != null)
                {
                    moveNode.Score = this.GetLead(combined.Scores, combined.GameState, move.PlayerToken);
                }

                if (singlePlayer != null && mainlines.Count > 1)
                {
                    var mainline = mainlines[0] = this.Maximize(singlePlayer, mainlines[0], mainlines[1]);
                    mainlines.RemoveAt(1);

                    // Alpha-beta pruning.
                    if (otherPlayer != null)
                    {
                        var scoresA = mainline.Scores;
                        var leadA = this.GetLead(scoresA, mainline.GameState, singlePlayer);

                        int comp;
                        if (alphaBetaScore.TryGetValue(singlePlayer, out var scoresB))
                        {
                            var leadB = this.GetLead(scoresB, state, singlePlayer);
                            comp = this.scoringMetric.Compare(leadA, leadB);
                        }
                        else
                        {
                            comp = 1;
                        }

                        if (comp > 0)
                        {
                            alphaBetaScore = alphaBetaScore.SetItem(singlePlayer, scoresA);
                            if (alphaBetaScore.TryGetValue(otherPlayer, out var scoresC))
                            {
                                var otherA = this.GetLead(scoresA, state, otherPlayer);
                                var leadC = this.GetLead(scoresC, state, otherPlayer);
                                comp = this.scoringMetric.Compare(leadC, otherA);

                                if (comp > 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            cancel.ThrowIfCancellationRequested();

            if (singlePlayer != null)
            {
                return mainlines[0];
            }
            else
            {
                var currentScore = this.scoringMetric.Score(node.State);
                var playerLeads = (from pm in allMoves.Select((m, i) => new { Move = m, Mainline = mainlines[i] })
                                   group pm.Mainline by pm.Move.PlayerToken into g
                                   let mainline = g.Aggregate((a, b) => this.Maximize(g.Key, a, b))
                                   select new
                                   {
                                       Mainline = mainline,
                                       PlayerToken = g.Key,
                                       Lead = this.GetLead(mainline, g.Key),
                                   }).ToList();
                var improvingMoves = (from pm in playerLeads
                                      let currentLead = this.GetLead(currentScore, state, pm.PlayerToken)
                                      let comp = this.scoringMetric.Compare(pm.Lead, currentLead)
                                      where comp > 0
                                      select pm).ToSet();

                if (improvingMoves.Count == 0)
                {
                    // TODO: Perhaps only the winning player would prefer to avoid a stalemate and would be the only player willing to play to avoid a stalemate.
                    improvingMoves = (from pm in playerLeads
                                      let currentLead = this.GetLead(currentScore, state, pm.PlayerToken)
                                      let comp = this.scoringMetric.Compare(pm.Lead, currentLead)
                                      where comp >= 0
                                      select pm).ToSet();
                }

                if (improvingMoves.Count == 0)
                {
                    // This is a stalemate? Without time controls, there is no incentive for any player to move.
                    var fullyDetermined = mainlines.All(m => m.FullyDetermined);
                    var depth = fullyDetermined ? mainlines.Max(m => m.Depth) : mainlines.Where(m => !m.FullyDetermined).Min(m => m.Depth);
                    return new Mainline<TMove, TScore>(this.scoringMetric.Score(state), state, null, ImmutableStack<IReadOnlyList<IWeighted<TMove>>>.Empty, depth, fullyDetermined);
                }
                else
                {
                    var added = true;
                    var remainingLeads = playerLeads.ToSet();
                    remainingLeads.ExceptWith(improvingMoves);

                    while (added && playerLeads.Count > 0)
                    {
                        added = false;

                        var preemptiveMoves = (from pm in remainingLeads
                                               where (from m in improvingMoves
                                                      let alternativeLead = this.GetLead(m.Mainline.Scores, state, pm.PlayerToken)
                                                      let comp = this.scoringMetric.Compare(pm.Lead, alternativeLead)
                                                      where comp > 0
                                                      select m).Any()
                                               select pm).ToList();

                        if (preemptiveMoves.Count > 0)
                        {
                            remainingLeads.ExceptWith(preemptiveMoves);
                            improvingMoves.UnionWith(preemptiveMoves);
                            added = true;
                        }
                    }

                    return this.CombineOutcomes(state, improvingMoves.Select(m => Weighted.Create(m.Mainline, 1)).ToList());
                }
            }
        }

        private Mainline<TMove, TScore> Maximize(PlayerToken player, Mainline<TMove, TScore> a, Mainline<TMove, TScore> b)
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
                return new Mainline<TMove, TScore>(a.Scores, a.GameState, a.PlayerToken, strategies, depth, fullyDetermined);
            }
            else if (comp > 0)
            {
                if (a.FullyDetermined == fullyDetermined && a.Depth == depth)
                {
                    return a;
                }

                return new Mainline<TMove, TScore>(a.Scores, a.GameState, a.PlayerToken, a.Strategies, depth, fullyDetermined);
            }
            else
            {
                if (b.FullyDetermined == fullyDetermined && b.Depth == depth)
                {
                    return b;
                }

                return new Mainline<TMove, TScore>(b.Scores, b.GameState, b.PlayerToken, b.Strategies, depth, fullyDetermined);
            }
        }
    }
}
