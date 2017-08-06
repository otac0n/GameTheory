﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TwentyFortyEight
{
    /// <summary>
    /// Represents a move in 2048.
    /// </summary>
    public abstract class Move : IMove
    {
        private readonly PlayerToken playerToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="playerToken">The player who may make this move.</param>
        protected Move(GameState state, PlayerToken playerToken)
        {
            this.State = state;
            this.playerToken = playerToken;
        }

        /// <inheritdoc/>
        public PlayerToken PlayerToken => this.playerToken;

        /// <inheritdoc/>
        public abstract bool IsDeterministic { get; }

        /// <summary>
        /// Gets the <see cref="GameState"/> that this move is based on.
        /// </summary>
        protected GameState State { get; }

        internal abstract GameState Apply(GameState state);
    }
}