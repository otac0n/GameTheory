// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents a move in Five Tribes.
    /// </summary>
    public abstract class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="playerToken">The player who may perform this move.</param>
        protected Move(GameState state, PlayerToken playerToken)
        {
            Contract.Requires(playerToken != null);

            this.State = state;
            this.PlayerToken = playerToken;
        }

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        public PlayerToken PlayerToken { get; private set; }

        internal GameState State { get; private set; }

        /// <inheritdoc />
        public abstract override string ToString();

        internal abstract GameState Apply(GameState state);
    }
}
