// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    /// <summary>
    /// Contains bit-masks for working with <see cref="Pieces"/>.
    /// </summary>
    public static class PieceMasks
    {
        /// <summary>
        /// A mask for the color of a piece.
        /// </summary>
        public const Pieces Colors = Pieces.White | Pieces.Black;

        /// <summary>
        /// A mask for the type of a piece.
        /// </summary>
        public const Pieces Piece = ~Colors;
    }
}
