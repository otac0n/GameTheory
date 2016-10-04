// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
{
    using System.Diagnostics.Contracts;
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
            Contract.Requires(state != null);

            this.State = state;
            this.Player = state.ActivePlayer;
        }

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        public PlayerToken Player { get; private set; }

        internal GameState State { get; private set; }

        /// <inheritdoc />
        public abstract override string ToString();

        internal virtual GameState Apply(GameState state)
        {
            // TODO: Implement transition moves for handling nobles and discarding tokens down to 10.
            state = state.With(
                activePlayer: state.Players[(state.Players.IndexOf(state.ActivePlayer) + 1) % state.Players.Count]);

            if (state.ActivePlayer == state.Players[0] && state.Players.Any(p => state.GetScore(p) >= GameState.ScoreLimit))
            {
                state = state.With(
                    phase: Phase.End);
            }

            return state;
        }
    }
}
