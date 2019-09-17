// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TwentyFortyEight
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a move in 2048.
    /// </summary>
    public abstract class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="TwentyFortyEight.GameState"/> that this move is based on.</param>
        /// <param name="playerToken">The player who may make this move.</param>
        protected Move(GameState state, PlayerToken playerToken)
        {
            this.GameState = state;
            this.PlayerToken = playerToken;
        }

        /// <inheritdoc/>
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc/>
        public abstract bool IsDeterministic { get; }

        /// <inheritdoc/>
        public PlayerToken PlayerToken { get; }

        internal GameState GameState { get; }

        /// <inheritdoc />
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal abstract GameState Apply(GameState state);

        internal virtual IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state) =>
            new IWeighted<GameState>[] { Weighted.Create(this.Apply(state), 1) };
    }
}
