// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.NotationSystems
{
    /// <summary>
    /// A variant of <see cref="AlgebraicNotation"/> where the pieces are replaced with their unicode counterparts.
    /// </summary>
    public class FigurineAlgebraicNotation : AlgebraicNotation
    {
        /// <inheritdoc />
        public override string Format(Pieces piece)
        {
            switch (piece)
            {
                case Pieces.Pawn | Pieces.Black:
                    return "\u265F";

                case Pieces.Pawn | Pieces.White:
                case Pieces.Pawn:
                    return "\u2659";

                case Pieces.Knight | Pieces.Black:
                    return "\u265E";

                case Pieces.Knight | Pieces.White:
                case Pieces.Knight:
                    return "\u2658";

                case Pieces.Bishop | Pieces.Black:
                    return "\u265D";

                case Pieces.Bishop | Pieces.White:
                case Pieces.Bishop:
                    return "\u2657";

                case Pieces.Rook | Pieces.Black:
                    return "\u265C";

                case Pieces.Rook | Pieces.White:
                case Pieces.Rook:
                    return "\u2656";

                case Pieces.Queen | Pieces.Black:
                    return "\u265B";

                case Pieces.Queen | Pieces.White:
                case Pieces.Queen:
                    return "\u2655";

                case Pieces.King | Pieces.Black:
                    return "\u265A";

                case Pieces.King | Pieces.White:
                case Pieces.King:
                    return "\u2654";
            }

            return string.Empty;
        }
    }
}
