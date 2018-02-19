// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

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
        private readonly int minPly;

        private readonly IScorePlyExtender<TScore> scoreExtender;
        private readonly IGameStateScoringMetric<TMove, TScore> scoringMetric;

        private ICache cache = new Caches.SplayTreeCache();

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
            this.minPly = minPly > -1 ? minPly : throw new ArgumentOutOfRangeException(nameof(minPly));
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

        private interface ICache
        {
            void Set(IGameState<TMove> state, Mainline result);

            void Trim();

            bool TryGetValue(IGameState<TMove> state, out Mainline cached);
        }

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <summary>
        /// Gets the maximum number of states to sample from all possible initial states.
        /// </summary>
        protected virtual int InitialSamples => 30;

        /// <summary>
        /// Gets a value indicating whether or not the player will calculate during an opponent's turn.
        /// </summary>
        protected virtual bool Ponder => true;

        /// <inheritdoc />
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel)
        {
            await Task.Yield();

            var moves = state.GetAvailableMoves();

            var ply = this.minPly;

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

            Mainline mainline;
            if (states.Count == 1)
            {
                mainline = this.GetMove(states.Single(), ply, cancel);
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
                this.cache.Trim();
                return mainline.Strategies.Peek().Pick();
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
            if (state.Players.Count == 1)
            {
                return score[player];
            }
            else
            {
                return this.scoringMetric.Difference(
                    score[player],
                    score.Where(s => s.Key != player).OrderByDescending(s => s.Value, this.scoringMetric).First().Value);
            }
        }

        private Mainline CombineOutcomes(IList<IWeighted<Mainline>> mainlines)
        {
            if (mainlines.Count == 1)
            {
                return mainlines[0].Value;
            }

            var maxWeight = double.NegativeInfinity;
            var maxPlayerToken = (PlayerToken)null;
            Mainline maxMainline = null;
            var fullyDetermined = true;

            var playerScores = new Dictionary<PlayerToken, IWeighted<TScore>[]>();

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

                foreach (var player in score.Keys)
                {
                    if (!playerScores.TryGetValue(player, out var playerScore))
                    {
                        playerScores[player] = playerScore = new IWeighted<TScore>[mainlines.Count];
                    }

                    playerScore[i] = Weighted.Create(score[player], weight);
                }
            }

            var combinedScore = playerScores.ToDictionary(ps => ps.Key, ps => this.scoringMetric.Combine(ps.Value));
            var depth = fullyDetermined ? mainlines.Max(m => m.Value.Depth) : mainlines.Where(m => !m.Value.FullyDetermined).Min(m => m.Value.Depth);
            return new Mainline(combinedScore, maxMainline.GameState, maxMainline.PlayerToken, maxMainline.Strategies, depth, fullyDetermined);
        }

        private TScore GetLead(Mainline mainline, PlayerToken player)
        {
            return this.GetLead(mainline.Scores, mainline.GameState, player);
        }

        private Mainline GetMaximizingMoves(PlayerToken player, IList<Mainline> moveScores)
        {
            var maxLead = default(Maybe<TScore>);
            var fullyDetermined = true;
            var maxMainlines = new List<Mainline>();
            for (var m = 0; m < moveScores.Count; m++)
            {
                var mainline = moveScores[m];
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

            var sourceMainline = maxMainlines.Pick();
            var maxMoves = maxMainlines.SelectMany(m => m.Strategies.Peek()).ToImmutableArray();
            var depth = fullyDetermined ? moveScores.Max(m => m.Depth) : moveScores.Where(m => !m.FullyDetermined).Min(m => m.Depth);
            return new Mainline(sourceMainline.Scores, sourceMainline.GameState, sourceMainline.PlayerToken, sourceMainline.Strategies.Pop().Push(maxMoves), depth, fullyDetermined);
        }

        private Mainline GetMove(IList<IGameState<TMove>> states, int ply, CancellationToken cancel)
        {
            var mainlines = new List<Mainline>(states.Count);
            var moveWeights = new Dictionary<TMove, IWeighted<Mainline>>(new ComparableEqualityComparer<TMove>());
            var fullyDetermined = true;

            foreach (var state in states)
            {
                var mainline = this.GetMove(state, ply - 1, cancel);
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
            var maxMoves = moveWeights.Select(m => Weighted.Create(m.Key, m.Value.Weight)).ToImmutableArray();
            var depth = fullyDetermined ? mainlines.Max(m => m.Depth) : mainlines.Where(m => !m.FullyDetermined).Min(m => m.Depth);
            return new Mainline(sourceMainline.Scores, sourceMainline.GameState, sourceMainline.PlayerToken, sourceMainline.Strategies.Pop().Push(maxMoves), depth, fullyDetermined);
        }

        private Mainline GetMove(IGameState<TMove> state, int ply, CancellationToken cancel)
        {
            if (this.cache.TryGetValue(state, out var cached))
            {
                if (cached.Depth >= ply || cached.FullyDetermined)
                {
                    return cached;
                }
            }

            Mainline result;
            if (ply == 0)
            {
                result = new Mainline(this.scoringMetric.Score(state), state, null, ImmutableStack<ImmutableArray<IWeighted<TMove>>>.Empty, depth: 0, fullyDetermined: false);
            }
            else
            {
                var allMoves = state.GetAvailableMoves();
                var players = allMoves.Select(m => m.PlayerToken).ToImmutableHashSet();

                // If only one player can move, they must choose a move.
                // If more than one player can move, then we assume that players will play moves that improve their position as well as moves that would result in a smaller loss than the best move of an opponent.
                // If this is a stalemate (or there are no moves), we return no move and score the current position (recurse with ply 0)
                if (players.Count == 0)
                {
                    result = new Mainline(this.scoringMetric.Score(state), state, null, ImmutableStack<ImmutableArray<IWeighted<TMove>>>.Empty, depth: 0, fullyDetermined: true);
                }
                else
                {
                    var moveScores = new Mainline[allMoves.Count];
                    for (var m = 0; m < allMoves.Count; m++)
                    {
                        var move = allMoves[m];
                        var outcomes = state.GetOutcomes(move);
                        moveScores[m] = this.CombineOutcomes(outcomes.Select(o =>
                        {
                            var mainline = this.GetMove(o.Value, ply - 1, cancel);
                            var newScores = this.scoreExtender != null
                                ? ImmutableDictionary.CreateRange(mainline.Scores.Select(kvp => new KeyValuePair<PlayerToken, TScore>(kvp.Key, this.scoreExtender.Extend(kvp.Value))))
                                : mainline.Scores;
                            var newMainline = new Mainline(newScores, mainline.GameState, move.PlayerToken, mainline.Strategies.Push(ImmutableArray.Create(Weighted.Create(move, 1))), mainline.Depth + 1, mainline.FullyDetermined);
                            return Weighted.Create(newMainline, o.Weight);
                        }).ToList());
                    }

                    cancel.ThrowIfCancellationRequested();

                    var playerMoves = (from pm in allMoves.Select((m, i) => new { Move = m, MoveScore = moveScores[i] })
                                       group pm.MoveScore by pm.Move.PlayerToken into g
                                       select new
                                       {
                                           Player = g.Key,
                                           Mainline = this.GetMaximizingMoves(g.Key, g.ToList()),
                                       }).ToList();

                    if (players.Count == 1)
                    {
                        result = playerMoves.Single().Mainline;
                    }
                    else
                    {
                        var currentScore = this.scoringMetric.Score(state);
                        var playerLeads = (from pm in playerMoves
                                           select new
                                           {
                                               pm.Mainline,
                                               pm.Player,
                                               Lead = this.GetLead(pm.Mainline, pm.Player),
                                           }).ToList();
                        var improvingMoves = (from pm in playerLeads
                                              let currentLead = this.GetLead(currentScore, state, pm.Player)
                                              let comp = this.scoringMetric.Compare(pm.Lead, currentLead)
                                              where comp > 0
                                              select pm).ToSet();

                        if (improvingMoves.Count == 0)
                        {
                            // TODO: Perhaps only the winning player would prefer to avoid a stalemate and would be the only player willing to play to avoid a stalemate.
                            improvingMoves = (from pm in playerLeads
                                              let currentLead = this.GetLead(currentScore, state, pm.Player)
                                              let comp = this.scoringMetric.Compare(pm.Lead, currentLead)
                                              where comp >= 0
                                              select pm).ToSet();
                        }

                        if (improvingMoves.Count == 0)
                        {
                            // This is a stalemate? Without time controls, there is no incentive for any player to move.
                            var fullyDetermined = moveScores.All(m => m.FullyDetermined);
                            var depth = fullyDetermined ? moveScores.Max(m => m.Depth) : moveScores.Where(m => !m.FullyDetermined).Min(m => m.Depth);
                            result = new Mainline(this.scoringMetric.Score(state), state, null, ImmutableStack<ImmutableArray<IWeighted<TMove>>>.Empty, depth, fullyDetermined);
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
                                                              let alternativeLead = this.GetLead(m.Mainline.Scores, state, pm.Player)
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

                            result = this.CombineOutcomes(improvingMoves.Select(m => Weighted.Create(m.Mainline, 1)).ToList());
                        }
                    }
                }
            }

            this.cache.Set(state, result);

            return result;
        }

        private static class Caches
        {
            public class DictionaryCache : ICache
            {
                private readonly Dictionary<IGameState<TMove>, Mainline> storage = new Dictionary<IGameState<TMove>, Mainline>();

                public void Set(IGameState<TMove> state, Mainline result) => this.storage[state] = result;

                public void Trim() => this.storage.Clear();

                public bool TryGetValue(IGameState<TMove> state, out Mainline cached) => this.storage.TryGetValue(state, out cached);
            }

            public class NullCache : ICache
            {
                public void Set(IGameState<TMove> state, Mainline result)
                {
                }

                public void Trim()
                {
                }

                public bool TryGetValue(IGameState<TMove> state, out Mainline cached)
                {
                    cached = default(Mainline);
                    return false;
                }
            }

            public class SplayTreeCache : ICache
            {
                private const int TrimDepth = 32;

                private readonly SplayTree<IGameState<TMove>, Mainline> storage = new SplayTree<IGameState<TMove>, Mainline>();

                public void Set(IGameState<TMove> state, Mainline result) => this.storage[state] = result;

                public void Trim() => this.storage.Trim(TrimDepth);

                public bool TryGetValue(IGameState<TMove> state, out Mainline cached) => this.storage.TryGetValue(state, out cached);
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

        /// <summary>
        /// Records a possible line of gameplay and it's computed scores.
        /// </summary>
        private class Mainline : ITokenFormattable
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Mainline"/> class.
            /// </summary>
            /// <param name="scores">The computed scores for all players in the resulting state.</param>
            /// <param name="state">The resulting game state.</param>
            /// <param name="strategies">The sequence of moves necessary to arrive at the resulting game state.</param>
            /// <param name="playerToken">The player who moves next in the sequence or <c>null</c> if there are no moves.</param>
            /// <param name="depth">The depth to which the score was computed.</param>
            public Mainline(IDictionary<PlayerToken, TScore> scores, IGameState<TMove> state, PlayerToken playerToken, ImmutableStack<ImmutableArray<IWeighted<TMove>>> strategies, int depth, bool fullyDetermined)
            {
                this.Scores = scores;
                this.GameState = state;
                this.PlayerToken = playerToken;
                this.Strategies = strategies;
                this.Depth = depth;
                this.FullyDetermined = fullyDetermined;
            }

            /// <summary>
            /// Gets the depth to which the score was computed.
            /// </summary>
            public int Depth { get; }

            /// <inheritdoc/>
            public IList<object> FormatTokens
            {
                get
                {
                    var tokens = new List<object>();

                    var first = true;
                    foreach (var strategy in this.Strategies)
                    {
                        if (!first)
                        {
                            tokens.Add("; ");
                        }

                        first = false;

                        var totalWeight = 0.0;
                        if (strategy.Length > 1)
                        {
                            tokens.Add("{");
                            totalWeight = strategy.Sum(m => m.Weight);
                        }

                        var firstMove = true;
                        foreach (var move in strategy)
                        {
                            if (!firstMove)
                            {
                                tokens.Add(", ");
                            }

                            firstMove = false;

                            tokens.Add(move.Value);

                            if (strategy.Length > 1)
                            {
                                tokens.Add(" @");
                                tokens.Add((move.Weight / totalWeight).ToString("P0"));
                            }
                        }

                        if (strategy.Length > 1)
                        {
                            tokens.Add("}");
                        }
                    }

                    tokens.Add(" [");

                    first = true;
                    foreach (var score in this.Scores)
                    {
                        if (!first)
                        {
                            tokens.Add(", ");
                        }

                        tokens.Add(score.Key);
                        tokens.Add(": ");
                        tokens.Add(score.Value);

                        first = false;
                    }

                    tokens.Add("] (depth ");

                    if (!this.FullyDetermined)
                    {
                        tokens.Add(">=");
                    }

                    tokens.Add(this.Depth);
                    tokens.Add(")");

                    return tokens;
                }
            }

            /// <summary>
            /// Gets a value indicating whether or not the entire game tree has been searched at this level.
            /// </summary>
            public bool FullyDetermined { get; }

            /// <summary>
            /// Gets the resulting game state.
            /// </summary>
            public IGameState<TMove> GameState { get; }

            /// <summary>
            /// Gets the player who moves next in the sequence or <c>null</c> if there are no moves.
            /// </summary>
            public PlayerToken PlayerToken { get; }

            /// <summary>
            /// Gets the computed scores for all players in the resulting state.
            /// </summary>
            public IDictionary<PlayerToken, TScore> Scores { get; }

            /// <summary>
            /// Gets the sequence of moves/strategies necessary to arrive at the resulting game state.
            /// </summary>
            public ImmutableStack<ImmutableArray<IWeighted<TMove>>> Strategies { get; }

            /// <inheritdoc/>
            public override string ToString() => string.Concat(this.FlattenFormatTokens());
        }
    }
}
