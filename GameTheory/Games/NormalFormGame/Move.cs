// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.NormalFormGame
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move in normal-form games.
    /// </summary>
    /// <typeparam name="T">The type representing the distint moves available.</typeparam>
    public sealed class Move<T> : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move{T}"/> class.
        /// </summary>
        /// <param name="playerToken">The player who may make this move.</param>
        /// <param name="kind">A value indicating the kind of move the player has chosen.</param>
        public Move(PlayerToken playerToken, T kind)
        {
            this.PlayerToken = playerToken;
            this.Kind = kind;
        }

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc />
        public bool IsDeterministic => true;

        /// <inheritdoc />
        public IList<object> FormatTokens => new object[] { this.Kind };

        /// <summary>
        /// Gets a value indicating the kind of move the player has chosen.
        /// </summary>
        public T Kind { get; }

        /// <inheritdoc />
        public override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
