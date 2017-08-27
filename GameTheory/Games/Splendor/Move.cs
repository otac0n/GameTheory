// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Splendor.
    /// </summary>
    public abstract class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        protected Move(GameState state)
        {
            this.State = state ?? throw new ArgumentOutOfRangeException(nameof(state));
            this.PlayerToken = state.ActivePlayer;
        }

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc />
        public abstract bool IsDeterministic { get; }

        /// <inheritdoc />
        public abstract IList<object> FormatTokens { get; }

        internal GameState State { get; }

        /// <inheritdoc />
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal virtual GameState Apply(GameState state)
        {
            if (Moves.DiscardTokensMove.ShouldTransitionToPhase(state))
            {
                return state.With(
                    phase: Phase.Discard);
            }

            if (Moves.ChooseNobleMove.ShouldTransitionToPhase(state))
            {
                return state.With(
                    phase: Phase.ChooseNoble);
            }

            state = state.With(
                activePlayer: state.Players[(state.Players.IndexOf(state.ActivePlayer) + 1) % state.Players.Count],
                phase: Phase.Play);

            if (state.ActivePlayer == state.Players[0] && state.Players.Any(p => state.GetScore(p) >= GameState.ScoreLimit))
            {
                state = state.With(
                    phase: Phase.End);
            }

            return state;
        }

        internal virtual IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state) => throw new NotImplementedException();
    }
}
