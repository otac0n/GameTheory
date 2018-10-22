namespace GameTheory.Games.Chess
{
    using System;

    /// <summary>
    /// Represents the state of a piece at a certain square.
    /// </summary>
    [Flags]
    public enum Pieces : byte
    {
        /// <summary>
        /// No piece is present.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// The piece present belongs to the white player.
        /// </summary>
        White = 0x01,

        /// <summary>
        /// The piece present belongs to the black player.
        /// </summary>
        Black = 0x02,

        /// <summary>
        /// Represents a Pawn.
        /// </summary>
        Pawn = 0x04,

        /// <summary>
        /// Represents a Knight.
        /// </summary>
        Knight = 0x08,

        /// <summary>
        /// Represents a Bishop.
        /// </summary>
        Bishop = 0x10,

        /// <summary>
        /// Represents a Rook.
        /// </summary>
        Rook = 0x20,

        /// <summary>
        /// Represents a Queen.
        /// </summary>
        Queen = 0x40,

        /// <summary>
        /// Represents a King.
        /// </summary>
        King = 0x80,
    }
}
