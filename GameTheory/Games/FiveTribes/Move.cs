// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using System;

    /// <summary>
    /// Represents a move in Five Tribes.
    /// </summary>
    public abstract class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.  The player for the move will be infered from the active player.</param>
        protected Move(GameState state)
            : this(state, state == null ? throw new ArgumentNullException(nameof(state)) : state.ActivePlayer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="playerToken">The player who may perform this move.</param>
        protected Move(GameState state, PlayerToken playerToken)
        {
            this.State = state;
            this.PlayerToken = playerToken ?? throw new ArgumentNullException(nameof(playerToken));
        }

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        public PlayerToken PlayerToken { get; }

        internal GameState State { get; }

        /// <inheritdoc />
        public abstract override string ToString();

        internal abstract GameState Apply(GameState state);
    }
}
