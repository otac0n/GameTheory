// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.$game$
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move in $game$.
    /// </summary>
    public sealed class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="playerToken">The player who may make this move.</param>
        public Move(PlayerToken playerToken)
        {
            this.PlayerToken = playerToken;
        }

        /// <inheritdoc />
        public IList<object> FormatTokens => throw new NotImplementedException();

        /// <inheritdoc />
        public bool IsDeterministic => true;

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc />
        public override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
