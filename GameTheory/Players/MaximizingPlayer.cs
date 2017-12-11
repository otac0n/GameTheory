﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players
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
        private const int TrimDepth = 32;
        private readonly SplayTree<IGameState<TMove>, Mainline> cache = new SplayTree<IGameState<TMove>, Mainline>();
        private readonly int minPly;
        private readonly IScoringMetric scoringMetric;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximizingPlayer{TMove, TScore}"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="scoringMetric">The scoring metric to use.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        protected MaximizingPlayer(PlayerToken playerToken, IScoringMetric scoringMetric, int minPly)
        {
            this.PlayerToken = playerToken;
            this.scoringMetric = scoringMetric ?? throw new ArgumentOutOfRangeException(nameof(scoringMetric));
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

        /// <summary>
        /// Provides an interface for the <see cref="MaximizingPlayer{TMove, TScore}"/> class to score game states.
        /// </summary>
        protected interface IScoringMetric : IComparer<TScore>
        {
            /// <summary>
            /// Combines one or more scores with the specified weights.
            /// </summary>
            /// <param name="scores">The weighted scores to combine.</param>
            /// <returns>The combines score.</returns>
            /// <remarks>
            /// <para>The length of scores and weights must be the same.</para>
            /// <para>If given a single score, that score should be returned without change.
            /// If given multiple scores, the function should return a expected score based on the specified weights.</para>
            /// <para>The player will use negative weights to subtract one score from another.</para>
            /// </remarks>
            TScore CombineScores(IWeighted<TScore>[] scores);

            /// <summary>
            /// Gets the difference between two players' scores.
            /// </summary>
            /// <param name="playerScore">The score to subtract from.</param>
            /// <param name="opponentScore">The score to subtract.</param>
            /// <returns>A score representing the difference between the specified scores.</returns>
            TScore Difference(TScore playerScore, TScore opponentScore);

            /// <summary>
            /// Scores a <see cref="IGameState{TMove}"/> for the specified player.
            /// </summary>
            /// <param name="state">The game state to score.</param>
            /// <param name="playerToken">The player whose score should be returned.</param>
            /// <returns>The player's score.</returns>
            TScore Score(IGameState<TMove> state, PlayerToken playerToken);
        }

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc />
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel)
        {
            await Task.Yield();

            var players = state.GetAvailableMoves().Select(m => m.PlayerToken).ToImmutableHashSet();
            if (!players.Contains(this.PlayerToken))
            {
                // We can avoid any further calculation at this state, because it is not our turn.
                // TODO: Consider "pondering."
                return default(Maybe<TMove>);
            }

            Mainline mainline;
            lock (this.cache)
            {
                mainline = this.GetMove(state, this.minPly, cancel);
                this.cache.Trim(TrimDepth);
            }

            if (mainline == null || !mainline.Moves.Any() || mainline.Moves.Peek().PlayerToken != this.PlayerToken)
            {
                return default(Maybe<TMove>);
            }
            else
            {
                this.MessageSent?.Invoke(this, new MessageSentEventArgs(mainline));
                return new Maybe<TMove>(mainline.Moves.Peek());
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

        private Mainline CombineOutcomes(IList<IWeighted<Mainline>> mainlines)
        {
            if (mainlines.Count == 1)
            {
                return mainlines[0].Value;
            }

            var maxWeight = default(Maybe<double>);
            var maxMainlines = new List<Mainline>();
            int minDepth = -1;

            var playerScores = new Dictionary<PlayerToken, IWeighted<TScore>[]>();

            for (var i = 0; i < mainlines.Count; i++)
            {
                var weightedMainline = mainlines[i];
                var weight = weightedMainline.Weight;
                var mainline = weightedMainline.Value;
                var score = mainline.Score;

                if (minDepth == -1 || mainline.Depth < minDepth)
                {
                    minDepth = mainline.Depth;
                }

                if (!maxWeight.HasValue)
                {
                    maxWeight = new Maybe<double>(weight);
                    maxMainlines.Add(mainline);
                }
                else if (weight >= maxWeight.Value)
                {
                    if (weight > maxWeight.Value)
                    {
                        maxWeight = new Maybe<double>(weight);
                        maxMainlines.Clear();
                    }

                    maxMainlines.Add(mainline);
                }

                foreach (var player in score.Keys)
                {
                    if (!playerScores.TryGetValue(player, out IWeighted<TScore>[] playerScore))
                    {
                        playerScores[player] = playerScore = new IWeighted<TScore>[mainlines.Count];
                    }

                    playerScore[i] = Weighted.Create(score[player], weight);
                }
            }

            var combinedScore = playerScores.ToImmutableDictionary(ps => ps.Key, ps => this.scoringMetric.CombineScores(ps.Value));

            var maxMainline = maxMainlines.Pick();
            return new Mainline(combinedScore, maxMainline.State, maxMainline.Moves, minDepth);
        }

        private TScore GetLead(Mainline mainline, PlayerToken player)
        {
            return this.GetLead(mainline.Score, mainline.State, player);
        }

        private TScore GetLead(IReadOnlyDictionary<PlayerToken, TScore> score, IGameState<TMove> state, PlayerToken player)
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

        private List<Mainline> GetMaximizingMoves(PlayerToken player, IList<Mainline> moveScores)
        {
            var maxLead = default(Maybe<TScore>);
            var maxMoves = new List<Mainline>();
            for (var m = 0; m < moveScores.Count; m++)
            {
                var mainline = moveScores[m];
                if (!maxLead.HasValue)
                {
                    maxLead = new Maybe<TScore>(this.GetLead(mainline, player));
                    maxMoves.Add(mainline);
                }
                else
                {
                    var lead = this.GetLead(mainline, player);
                    var comp = this.scoringMetric.Compare(lead, maxLead.Value);
                    if (comp >= 0)
                    {
                        if (comp > 0)
                        {
                            maxLead = new Maybe<TScore>(lead);
                            maxMoves.Clear();
                        }

                        maxMoves.Add(mainline);
                    }
                }
            }

            return maxMoves;
        }

        private Mainline GetMove(IGameState<TMove> state, int ply, CancellationToken cancel)
        {
            if (this.cache.TryGetValue(state, out Mainline cached))
            {
                if (cached.Depth >= ply)
                {
                    return cached;
                }
            }

            if (ply == 0)
            {
                return this.cache[state] = new Mainline(this.Score(state), state, ImmutableStack<TMove>.Empty, 0);
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
                    return this.cache[state] = new Mainline(this.Score(state), state, ImmutableStack<TMove>.Empty, ply);
                }

                var moveScores = new Mainline[allMoves.Count];
                for (var m = 0; m < allMoves.Count; m++)
                {
                    var move = allMoves[m];
                    var outcomes = state.GetOutcomes(move);
                    var mainlines = outcomes.Select(o => Weighted.Create(this.GetMove(o.Value, ply - 1, cancel).AddMove(move), o.Weight));
                    moveScores[m] = this.CombineOutcomes(mainlines.ToList());
                }

                cancel.ThrowIfCancellationRequested();

                var playerMoves = (from pm in allMoves.Select((m, i) => new { Move = m, MoveScore = moveScores[i] })
                                   group pm.MoveScore by pm.Move.PlayerToken into g
                                   select new
                                   {
                                       Player = g.Key,
                                       MaxMove = this.GetMaximizingMoves(g.Key, g.ToList()).Pick(),
                                   }).ToList();

                if (players.Count == 1)
                {
                    return this.cache[state] = playerMoves.Single().MaxMove;
                }
                else
                {
                    var currentScore = this.Score(state);
                    var playerLeads = (from pm in playerMoves
                                       select new
                                       {
                                           pm.MaxMove,
                                           pm.Player,
                                           Lead = this.GetLead(pm.MaxMove, pm.Player),
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

                        if (improvingMoves.Count == 0)
                        {
                            // This is a stalemate? Without time controls, there is no incentive for any player to move.
                            return this.cache[state] = new Mainline(this.Score(state), state, ImmutableStack<TMove>.Empty, ply);
                        }
                    }

                    var added = true;
                    var remainingLeads = playerLeads.ToSet();
                    remainingLeads.ExceptWith(improvingMoves);

                    while (added && playerLeads.Count > 0)
                    {
                        added = false;

                        var preemptiveMoves = (from pm in remainingLeads
                                               where (from m in improvingMoves
                                                      let alternativeLead = this.GetLead(m.MaxMove.Score, state, pm.Player)
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

                    // TODO: We should typically choose our own move if available.
                    var outcome = this.CombineOutcomes(improvingMoves.Select(m => Weighted.Create(m.MaxMove, 1)).ToList());
                    return this.cache[state] = outcome;
                }
            }
        }

        private IReadOnlyDictionary<PlayerToken, TScore> Score(IGameState<TMove> state)
        {
            return state.Players.ToImmutableDictionary(p => p, p => this.scoringMetric.Score(state, p));
        }

        private class Mainline : ITokenFormattable
        {
            public Mainline(IReadOnlyDictionary<PlayerToken, TScore> score, IGameState<TMove> state, ImmutableStack<TMove> moves, int depth)
            {
                this.Score = score;
                this.State = state;
                this.Moves = moves;
                this.Depth = depth;
            }

            public int Depth { get; }

            public IList<object> FormatTokens
            {
                get
                {
                    var tokens = new List<object>();

                    var first = true;
                    foreach (var move in this.Moves)
                    {
                        if (!first)
                        {
                            tokens.Add(" ");
                        }

                        tokens.Add(move);
                        first = false;
                    }

                    tokens.Add(" [");

                    first = true;
                    foreach (var score in this.Score)
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

                    tokens.Add("]");

                    return tokens;
                }
            }

            public ImmutableStack<TMove> Moves { get; }

            public IReadOnlyDictionary<PlayerToken, TScore> Score { get; }

            public IGameState<TMove> State { get; }

            public Mainline AddMove(TMove move)
            {
                return new Mainline(
                    this.Score,
                    this.State,
                    this.Moves.Push(move),
                    this.Depth + 1);
            }

            public override string ToString() => string.Concat(this.FlattenFormatTokens());
        }
    }
}
