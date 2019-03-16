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
    using GameTheory.Players.MaximizingPlayer.Caches;

    /// <summary>
    /// Implements a player that maximizes a scoring function to some move depth (ply).
    /// </summary>
    /// <typeparam name="TMove">The type of move that the player supports.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    /// <remarks>
    /// The maximizing player is a generalization of the min-max concept to games that have more than two players and that may allow moves by more than one player at a time.
    /// </remarks>
    public abstract class MaximizingPlayer<TMove, TScore> : IPlayer<TMove>
        where TMove : IMove
    {
        private readonly GameTree<TMove, TScore> gameTree;

        private readonly IScorePlyExtender<TScore> scoreExtender;

        private readonly IGameStateScoringMetric<TMove, TScore> scoringMetric;
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
        {
            this.PlayerToken = playerToken;
            this.scoringMetric = scoringMetric ?? throw new ArgumentNullException(nameof(scoringMetric));
            this.scoreExtender = this.scoringMetric as IScorePlyExtender<TScore>;
            this.MinPly = minPly > -1 ? minPly : throw new ArgumentOutOfRangeException(nameof(minPly));
            this.gameTree = new GameTree<TMove, TScore>(this.MakeCache());
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MaximizingPlayer{TMove, TScore}"/> class.
        /// </summary>
        ~MaximizingPlayer()
        {
            this.Dispose(false);
        }

        /// <inheritdoc />
        public event EventHandler<MessageSentEventArgs> MessageSent;

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <summary>
        /// Gets the maximum number of states to sample from all possible initial states.
        /// </summary>
        protected virtual int InitialSamples => 30;

        /// <summary>
        /// Gets the minimum number of ply to think ahead.
        /// </summary>
        protected int MinPly { get; }

        /// <summary>
        /// Gets a value indicating whether or not the player will calculate during an opponent's turn.
        /// </summary>
        protected virtual bool Ponder => true;

        /// <inheritdoc />
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel)
        {
            await Task.Yield();

            var sw = Stopwatch.StartNew();
            var moves = state.GetAvailableMoves();

            var ply = this.MinPly;

            var players = moves.Select(m => m.PlayerToken).ToImmutableHashSet();
            if (!players.Contains(this.PlayerToken))
            {
                if (!this.Ponder || moves.Count == 0)
                {
                    return default(Maybe<TMove>);
                }
                else
                {
                    ply += 1;
                }
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

            this.nodesSearched = 0;
            this.cacheHits = 0;
            this.cacheMisses = 0;

            Mainline<TMove, TScore> mainline;
            if (states.Count == 1)
            {
                mainline = this.GetMove(this.gameTree.GetOrAdd(states.Single()), ply, ImmutableDictionary<PlayerToken, IDictionary<PlayerToken, TScore>>.Empty, cancel);
            }
            else
            {
                mainline = this.GetMove(states, ply, cancel);
            }

            if (mainline == null || mainline.PlayerToken != this.PlayerToken)
            {
                return default(Maybe<TMove>);
            }
            else
            {
                this.MessageSent?.Invoke(this, new MessageSentEventArgs(mainline));
                var chosen = mainline.Strategies.Peek().Pick();
                sw.Stop();
                this.MessageSent?.Invoke(this, new MessageSentEventArgs("Time taken: ", sw.ElapsedMilliseconds, " ms, Nodes searched: ", this.nodesSearched, " (", (this.nodesSearched / sw.Elapsed.TotalMilliseconds).ToString("F2"), " kn/s), Cache hits: ", ((double)this.cacheHits / this.nodesSearched).ToString("P"), ", Cache misses: ", ((double)this.cacheMisses / this.nodesSearched).ToString("P")));
                this.gameTree.Trim();
                return chosen;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Component and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
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
        protected virtual TScore GetLead(IDictionary<PlayerToken, TScore> score, IGameState<TMove> state, PlayerToken player)
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
                var max = default(TScore);
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
        protected virtual ICache<TMove, TScore> MakeCache() => new SplayTreeCache<TMove, TScore>();

        private Mainline<TMove, TScore> CombineOutcomes(IList<Weighted<Mainline<TMove, TScore>>> mainlines)
        {
            var firstMainline = mainlines[0].Value;
            if (mainlines.Count == 1)
            {
                return firstMainline;
            }

            var maxWeight = double.NegativeInfinity;
            var maxPlayerToken = (PlayerToken)null;
            Mainline<TMove, TScore> maxMainline = null;
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

            var combinedScore = new Dictionary<PlayerToken, TScore>();
            var playerScore = new Weighted<TScore>[mainlines.Count];
            foreach (var player in firstMainline.GameState.Players)
            {
                for (var i = 0; i < mainlines.Count; i++)
                {
                    playerScore[i] = Weighted.Create(mainlines[i].Value.Scores[player], mainlines[i].Weight);
                }

                combinedScore[player] = this.scoringMetric.Combine(playerScore);
            }

            var depth = fullyDetermined ? mainlines.Max(m => m.Value.Depth) : mainlines.Where(m => !m.Value.FullyDetermined).Min(m => m.Value.Depth);
            return new Mainline<TMove, TScore>(combinedScore, maxMainline.GameState, maxMainline.PlayerToken, maxMainline.Strategies, depth, fullyDetermined);
        }

        private TScore GetLead(Mainline<TMove, TScore> mainline, PlayerToken player) => this.GetLead(mainline.Scores, mainline.GameState, player);

        private Mainline<TMove, TScore> GetMove(IList<IGameState<TMove>> states, int ply, CancellationToken cancel)
        {
            var mainlines = new List<Mainline<TMove, TScore>>(states.Count);
            var moveWeights = new Dictionary<TMove, Weighted<Mainline<TMove, TScore>>>(new ComparableEqualityComparer<TMove>());
            var fullyDetermined = true;

            // Monte-Carlo
            foreach (var state in states)
            {
                var mainline = this.GetMove(this.gameTree.GetOrAdd(state), ply - 1, ImmutableDictionary<PlayerToken, IDictionary<PlayerToken, TScore>>.Empty, cancel);
                mainlines.Add(mainline);
                fullyDetermined &= mainline.FullyDetermined;

                var strategy = mainline.Strategies.Peek();
                foreach (var move in strategy)
                {
                    if (moveWeights.TryGetValue(move.Value, out var weighted))
                    {
                        weighted = Weighted.Create(weighted.Value, weighted.Weight + move.Weight);
                    }
                    else
                    {
                        weighted = Weighted.Create(mainline, move.Weight);
                    }

                    moveWeights[move.Value] = weighted;
                }
            }

            var sourceMainline = mainlines.Pick();
            var maxMoves = moveWeights.Select(m => (IWeighted<TMove>)Weighted.Create(m.Key, m.Value.Weight)).ToImmutableArray();
            var depth = fullyDetermined ? mainlines.Max(m => m.Depth) : mainlines.Where(m => !m.FullyDetermined).Min(m => m.Depth);
            var scores = (from m in mainlines
                          from s in m.Scores
                          group s.Value by s.Key into g
                          select new
                          {
                              PlayerToken = g.Key,
                              Score = this.scoringMetric.Combine(g.Select(s => Weighted.Create(s, 1)).ToArray()),
                          }).ToDictionary(m => m.PlayerToken, m => m.Score);
            return new Mainline<TMove, TScore>(scores, sourceMainline.GameState, sourceMainline.PlayerToken, sourceMainline.Strategies.Pop().Push(maxMoves), depth, fullyDetermined);
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

                    var newScores = mainline.Scores;
                    if (this.scoreExtender != null)
                    {
                        var scores = new Dictionary<PlayerToken, TScore>();

                        foreach (var player in mainline.GameState.Players)
                        {
                            scores.Add(player, this.scoreExtender.Extend(mainline.Scores[player]));
                        }

                        newScores = scores;
                    }

                    var newMainline = new Mainline<TMove, TScore>(newScores, mainline.GameState, move.PlayerToken, mainline.Strategies.Push(ImmutableArray.Create<IWeighted<TMove>>(Weighted.Create(move, 1))), mainline.Depth + 1, mainline.FullyDetermined);
                    weightedOutcomes.Add(Weighted.Create(newMainline, outcome.Weight));
                }

                var combined = this.CombineOutcomes(weightedOutcomes);
                mainlines.Add(combined);
                moveNode.Lead = this.GetLead(combined.Scores, combined.GameState, move.PlayerToken);

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

                    return this.CombineOutcomes(improvingMoves.Select(m => Weighted.Create(m.Mainline, 1)).ToList());
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

        private class ComparableEqualityComparer<T> : IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
            {
                IComparable<T> comparable;
                if (object.ReferenceEquals(null, x))
                {
                    return object.ReferenceEquals(null, y);
                }
                else if ((comparable = x as IComparable<T>) != null)
                {
                    return comparable.CompareTo(y) == 0;
                }
                else
                {
                    return x.Equals(y);
                }
            }

            public int GetHashCode(T obj) => 0;
        }
    }
}
