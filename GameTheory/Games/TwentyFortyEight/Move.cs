// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TwentyFortyEight
{
    using System.Collections.Generic;

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
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc/>
        public abstract bool IsDeterministic { get; }

        /// <inheritdoc/>
        public PlayerToken PlayerToken => this.playerToken;

        /// <summary>
        /// Gets the <see cref="GameState"/> that this move is based on.
        /// </summary>
        internal GameState State { get; }

        /// <inheritdoc />
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal abstract GameState Apply(GameState state);
    }
}
