﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        private readonly int minPly;
        private readonly PlayerToken playerToken;
        private readonly IScoringMetric scoringMetric;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximizingPlayer{TMove, TScore}"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="scoringMetric">The scoring metric to use.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        protected MaximizingPlayer(PlayerToken playerToken, IScoringMetric scoringMetric, int minPly)
        {
            this.playerToken = playerToken;
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

        /// <summary>
        /// Provides an interface for the <see cref="MaximizingPlayer{TMove, TScore}"/> class to score game states.
        /// </summary>
        protected interface IScoringMetric : IComparer<TScore>
        {
            /// <summary>
            /// Combines one or more scores with the specified weights.
            /// </summary>
            /// <param name="scores">The scores to combine.</param>
            /// <param name="weights">The weights to use when combining</param>
            /// <returns>The combines score.</returns>
            /// <remarks>
            /// <para>The length of scores and weights must be the same.</para>
            /// <para>If given a single score, that score should be returned without change.
            /// If given multiple scores, the function should return a expected score based on the specified weights.</para>
            /// <para>The player will use negative weights to subtract one score from another.</para>
            /// </remarks>
            TScore CombineScores(TScore[] scores, double[] weights);

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
        public PlayerToken PlayerToken => this.playerToken;

        /// <inheritdoc />
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel)
        {
            await Task.Yield();

            var mainline = this.GetMove(state, this.minPly, cancel);

            if (!mainline.Moves.Any() || mainline.Moves.Peek().PlayerToken != this.playerToken)
            {
                return default(Maybe<TMove>);
            }
            else
            {
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

        private Mainline GetMove(IGameState<TMove> state, int ply, CancellationToken cancel)
        {
            if (ply == 0)
            {
                return new Mainline(this.Score(state), state, ImmutableStack<TMove>.Empty);
            }
            else
            {
                var allMoves = state.GetAvailableMoves();
                var moveScores = (from m in allMoves
                                  let nextState = state.MakeMove(m)
                                  select this.GetMove(nextState, ply - 1, cancel).AddMove(m)).ToList();
                cancel.ThrowIfCancellationRequested();

                // If only one player can move, they must choose a move.
                // If more than one player can move, then we assume that players will exclusively play moves that improve their position.
                // If this is a stalemate (or there are no moves), we return no move and score the current position (recurse with ply 0)
                var players = moveScores.Select(m => m.Moves.Peek().PlayerToken).ToImmutableHashSet();
                if (players.Count == 1)
                {
                    var player = players.First();
                    return moveScores
                        .OrderByDescending(m => this.GetLead(m, player), this.scoringMetric)
                        .First();
                }
                else if (players.Count == 0)
                {
                    return this.GetMove(state, 0, cancel);
                }
                else
                {
                    var currentScore = this.Score(state);
                    throw new NotImplementedException();
                }
            }
        }

        private IReadOnlyDictionary<PlayerToken, TScore> Score(IGameState<TMove> state)
        {
            return state.Players.ToImmutableDictionary(p => p, p => this.scoringMetric.Score(state, p));
        }

        private TScore GetLead(Mainline mainline, PlayerToken player)
        {
            if (mainline.State.Players.Count == 1)
            {
                return mainline.Score[player];
            }
            else
            {
                return this.scoringMetric.Difference(
                    mainline.Score[player],
                    mainline.Score.Where(s => s.Key != player).OrderByDescending(s => s.Value, this.scoringMetric).First().Value);
            }
        }

        private class Mainline
        {
            public Mainline(IReadOnlyDictionary<PlayerToken, TScore> score, IGameState<TMove> state, ImmutableStack<TMove> moves)
            {
                this.Score = score;
                this.State = state;
                this.Moves = moves;
            }

            public IGameState<TMove> State { get; }

            public ImmutableStack<TMove> Moves { get; }

            public IReadOnlyDictionary<PlayerToken, TScore> Score { get; }

            public Mainline AddMove(TMove move)
            {
                return new Mainline(
                    this.Score,
                    this.State,
                    this.Moves.Push(move));
            }
        }
    }
}
