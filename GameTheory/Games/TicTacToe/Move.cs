// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TicTacToe
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a move in Tic-tac-toe.
    /// </summary>
    public sealed class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="playerToken">The player who may make this move.</param>
        /// <param name="x">The x coordinate of the spot on which the move will me made.</param>
        /// <param name="y">The y coordinate of the spot on which the move will me made.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public Move(PlayerToken playerToken, int x, int y)
        {
            this.PlayerToken = playerToken;
            this.X = x;
            this.Y = y;
        }

        /// <inheritdoc />
        public IList<object> FormatTokens => new object[] { "(", this.X, ", ", this.Y, ")" };

        /// <inheritdoc />
        public bool IsDeterministic => true;

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <summary>
        /// Gets the x coordinate of the spot on which the move will me made.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "X is meaningful in the context of coordinates.")]
        public int X { get; }

        /// <summary>
        /// Gets the y coordinate of the spot on which the move will me made.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y", Justification = "Y is meaningful in the context of coordinates.")]
        public int Y { get; }

        /// <inheritdoc />
        public override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
