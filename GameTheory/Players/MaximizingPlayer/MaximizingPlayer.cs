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

    /// <summary>
    /// Implements a player that maximizes a scoring function to some move depth (ply).
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that the player will evaluate.</typeparam>
    /// <typeparam name="TMove">The type of moves that the player will choose.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    /// <remarks>
    /// The maximizing player is a generalization of the min-max concept to games that have more than two players and that may allow moves by more than one player at a time.
    /// </remarks>
    public abstract class MaximizingPlayer<TGameState, TMove, TScore> : MaximizingPlayerBase<TGameState, TMove, TScore>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        private int cacheHits;
        private int cacheMisses;
        private int nodesSearched;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximizingPlayer{TGameState, TMove, TScore}"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="scoringMetric">The scoring metric to use.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        protected MaximizingPlayer(PlayerToken playerToken, IGameStateScoringMetric<TGameState, TMove, TScore> scoringMetric, int minPly)
            : base(playerToken, scoringMetric)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(minPly);

            this.MinPly = minPly;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MaximizingPlayer{TGameState, TMove, TScore}"/> class.
        /// </summary>
        ~MaximizingPlayer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the minimum number of ply to think ahead.
        /// </summary>
        protected int MinPly { get; }

        /// <summary>
        /// Gets a value indicating the minimum amount of time to think in the case that the min ply is reached quickly.
        /// </summary>
        protected virtual TimeSpan? MinThinkTime => null;

        /// <inheritdoc />
        protected override Mainline<TGameState, TMove, TScore> GetMove(List<TGameState> states, bool ponder, CancellationToken cancel)
        {
            var sw = Stopwatch.StartNew();

            var runUntilCancel = false;
            var minTime = this.MinThinkTime;
            if (minTime != null)
            {
                runUntilCancel = true;
            }

            var ply = this.MinPly;
            if (ponder)
            {
                ply += 1;
            }

            this.nodesSearched = 0;
            this.cacheHits = 0;
            this.cacheMisses = 0;

            var mainlines = new Mainline<TGameState, TMove, TScore>[states.Count];
            var roots = states.Select(this.gameTree.GetOrAdd).ToList();

            if (states.Count > 1)
            {
                ply = Math.Max(1, ply - 1);
            }

            // Iterative deepening
            var fullyDetermined = false;
            for (var p = ply; (p <= ply || (runUntilCancel && !cancel.IsCancellationRequested)) && !fullyDetermined; p++)
            {
                fullyDetermined = true;
                for (var i = 0; i < states.Count; i++)
                {
                    var m = mainlines[i];

                    if (m == null || (!m.FullyDetermined && m.Depth < p))
                    {
                        m = mainlines[i] = this.GetMove(roots[i], p, ImmutableDictionary<PlayerToken, TScore>.Empty, cancel);
                    }

                    fullyDetermined &= m.FullyDetermined;
                }

                if (runUntilCancel && p == ply)
                {
                    var cancelAfter = minTime.Value - sw.Elapsed;
                    if (cancelAfter <= TimeSpan.Zero)
                    {
                        break;
                    }

                    var cts = new CancellationTokenSource();
                    cancel.Register(cts.Cancel);
                    cts.CancelAfter(cancelAfter);
                    cancel = cts.Token;
                }
            }

            var mainline = this.CombineMainlines(mainlines);

            if (!ponder)
            {
                this.SendMessage(mainline);
                this.SendMessage("Time taken: ", sw.ElapsedMilliseconds, " ms, Nodes searched: ", this.nodesSearched, " (", (this.nodesSearched / sw.Elapsed.TotalMilliseconds).ToString("F2"), " kn/s), Cache hits: ", ((double)this.cacheHits / this.nodesSearched).ToString("P"), ", Cache misses: ", ((double)this.cacheMisses / this.nodesSearched).ToString("P"));
            }

            return mainline;
        }

        /// <inheritdoc />
        protected override IGameStateCache<TGameState, TMove, TScore> MakeCache() => new SplayTreeCache<TGameState, TMove, TScore>();

        private Mainline<TGameState, TMove, TScore> GetMove(StateNode<TGameState, TMove, TScore> node, int ply, ImmutableDictionary<PlayerToken, TScore> alphaBetaLeads, CancellationToken cancel)
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

            return node.Mainline = this.GetMoveImpl(node, ply, alphaBetaLeads, cancel);
        }

        private Mainline<TGameState, TMove, TScore> GetMoveImpl(StateNode<TGameState, TMove, TScore> node, int ply, ImmutableDictionary<PlayerToken, TScore> alphaBetaLeads, CancellationToken cancel)
        {
            var state = node.State;

            if (cancel.IsCancellationRequested && node.Mainline != null)
            {
                return node.Mainline;
            }
            else if (cancel.IsCancellationRequested || ply == 0)
            {
                return new Mainline<TGameState, TMove, TScore>(this.scoringMetric.Score(state), state, null, ImmutableStack<IReadOnlyList<IWeighted<TMove>>>.Empty, depth: 0, fullyDetermined: false);
            }

            var allMoves = node.Moves;
            var players = allMoves.Select(m => m.PlayerToken).ToImmutableHashSet();

            // For Alpha-beta pruning.
            PlayerToken? singlePlayer, otherPlayer;

            // If only one player can move, they must choose a move.
            // If more than one player can move, then we assume that players will play moves that improve their position as well as moves that would result in a smaller loss than the best move of an opponent.
            // If this is a stalemate (or there are no moves), we return no move and score the current position (recurse with ply 0)
            switch (players.Count)
            {
                case 0:
                    return new Mainline<TGameState, TMove, TScore>(this.scoringMetric.Score(state), state, null, ImmutableStack<IReadOnlyList<IWeighted<TMove>>>.Empty, depth: 0, fullyDetermined: true);

                case 1:
                    singlePlayer = players.Single();
                    otherPlayer = state.Players.Count == 2 ? state.GetNextPlayer<TGameState, TMove>(singlePlayer) : null;
                    break;

                default:
                    singlePlayer = null;
                    otherPlayer = null;
                    break;
            }

            if (otherPlayer != null)
            {
                // TODO: Allow move sorting by inheriting classes.
                ////Array.Sort(allMoves, (m1, m2) => this.scoringMetric.Compare(node[m1].Score, node[m2].Score));
            }

            var mainlines = new Mainline<TGameState, TMove, TScore>[allMoves.Length];
            var leads = new TScore[allMoves.Length];
            for (var m = 0; m < allMoves.Length; m++)
            {
                var move = allMoves[m];
                var moveNode = node[move];

                // Expectimax.
                var outcomes = moveNode.Outcomes;
                var weightedOutcomes = new List<Weighted<Mainline<TGameState, TMove, TScore>>>();
                foreach (var outcome in outcomes)
                {
                    if (this.scoreExtender != null)
                    {
                        alphaBetaLeads = alphaBetaLeads.ToImmutableDictionary(kvp => kvp.Key, kvp => this.scoreExtender.Reduce(kvp.Value));
                    }

                    var mainline = this.GetMove(outcome.Value, ply - 1, alphaBetaLeads, cancel);
                    var strategy = ImmutableArray.Create<IWeighted<TMove>>(Weighted.Create(move, 1));
                    var newMainline = mainline.Extend(move.PlayerToken, strategy, this.scoreExtender);
                    weightedOutcomes.Add(Weighted.Create(newMainline, outcome.Weight));
                }

                var combined = this.CombineOutcomes(state, weightedOutcomes);
                mainlines[m] = combined;
                moveNode.Score = leads[m] = this.GetLead(combined.Scores, combined.GameState, move.PlayerToken);

                if (singlePlayer != null)
                {
                    if (m > 0)
                    {
                        // Basic negamax. Not used for simultaneous moves.
                        mainlines[0] = this.Maximize(singlePlayer, mainlines[0], leads[0], mainlines[m], leads[m]);
                        leads[0] = this.GetLead(mainlines[0].Scores, mainlines[0].GameState, singlePlayer);
                    }

                    // Alpha-beta pruning.
                    if (otherPlayer != null)
                    {
                        var otherLead = this.GetLead(mainlines[0].Scores, mainlines[0].GameState, otherPlayer);
                        if (alphaBetaLeads.TryGetValue(otherPlayer, out var beta))
                        {
                            if (this.scoringMetric.Compare(beta, otherLead) >= 0)
                            {
                                break;
                            }
                        }

                        if (!alphaBetaLeads.TryGetValue(singlePlayer, out var alpha) || this.scoringMetric.Compare(leads[0], alpha) > 0)
                        {
                            alphaBetaLeads = alphaBetaLeads.SetItem(singlePlayer, alpha = leads[0]);
                        }
                    }
                }
            }

            if (singlePlayer != null)
            {
                return mainlines[0];
            }
            else
            {
                var currentScore = this.scoringMetric.Score(node.State);
                var playerLeads = (from pm in allMoves.Select((m, i) => new { Move = m, Index = i })
                                   let mainline = mainlines[pm.Index]
                                   let lead = leads[pm.Index]
                                   let player = pm.Move.PlayerToken
                                   group new { Mainline = mainline, Lead = lead } by player into g
                                   let max = g.Aggregate((a, b) =>
                                   {
                                       var m = this.Maximize(g.Key, a.Mainline, a.Lead, b.Mainline, b.Lead);
                                       return new { Mainline = m, Lead = this.GetLead(m.Scores, m.GameState, g.Key) };
                                   })
                                   select new
                                   {
                                       max.Mainline,
                                       PlayerToken = g.Key,
                                       Lead = this.GetLead(max.Mainline, g.Key),
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
                    return new Mainline<TGameState, TMove, TScore>(this.scoringMetric.Score(state), state, null, ImmutableStack<IReadOnlyList<IWeighted<TMove>>>.Empty, depth, fullyDetermined);
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

        private Mainline<TGameState, TMove, TScore> Maximize(PlayerToken player, Mainline<TGameState, TMove, TScore> a, TScore leadA, Mainline<TGameState, TMove, TScore> b, TScore leadB)
        {
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
                return new Mainline<TGameState, TMove, TScore>(a.Scores, a.GameState, a.PlayerToken, strategies, depth, fullyDetermined);
            }
            else if (comp > 0)
            {
                if (a.FullyDetermined == fullyDetermined && a.Depth == depth)
                {
                    return a;
                }

                return new Mainline<TGameState, TMove, TScore>(a.Scores, a.GameState, a.PlayerToken, a.Strategies, depth, fullyDetermined);
            }
            else
            {
                if (b.FullyDetermined == fullyDetermined && b.Depth == depth)
                {
                    return b;
                }

                return new Mainline<TGameState, TMove, TScore>(b.Scores, b.GameState, b.PlayerToken, b.Strategies, depth, fullyDetermined);
            }
        }
    }
}
