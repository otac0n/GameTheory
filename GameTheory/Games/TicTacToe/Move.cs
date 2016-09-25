// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TicTacToe
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a move in Tic-tac-toe.
    /// </summary>
    public struct Move : IMove
    {
        private readonly PlayerToken player;
        private readonly int x;
        private readonly int y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> struct.
        /// </summary>
        /// <param name="player">The player who may make this move.</param>
        /// <param name="x">The x coordinate of the spot on which the move will me made.</param>
        /// <param name="y">The y coordinate of the spot on which the move will me made.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public Move(PlayerToken player, int x, int y)
        {
            this.player = player;
            this.x = x;
            this.y = y;
        }

        /// <inheritdoc />
        public PlayerToken Player
        {
            get { return this.player; }
        }

        /// <summary>
        /// Gets the x coordinate of the spot on which the move will me made.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "X is meaningful in the context of coordinates.")]
        public int X
        {
            get { return this.x; }
        }

        /// <summary>
        /// Gets the y coordinate of the spot on which the move will me made.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y", Justification = "Y is meaningful in the context of coordinates.")]
        public int Y
        {
            get { return this.y; }
        }
    }
}
