// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.NotationSystems
{
    using System.Collections.Generic;
    using GameTheory.Games.Chess.Moves;

    /// <summary>
    /// A coordinate based notation for representing moves.
    /// </summary>
    public class ShortCoordinateNotation : NotationSystem
    {
        /// <inheritdoc />
        public override string Format(Pieces piece)
        {
            switch (piece)
            {
                case Pieces.Pawn:
                case Pieces.Pawn | Pieces.Black:
                case Pieces.Pawn | Pieces.White:
                    return "p";

                case Pieces.Knight:
                case Pieces.Knight | Pieces.Black:
                case Pieces.Knight | Pieces.White:
                    return "n";

                case Pieces.Bishop:
                case Pieces.Bishop | Pieces.Black:
                case Pieces.Bishop | Pieces.White:
                    return "b";

                case Pieces.Rook:
                case Pieces.Rook | Pieces.Black:
                case Pieces.Rook | Pieces.White:
                    return "r";

                case Pieces.Queen:
                case Pieces.Queen | Pieces.Black:
                case Pieces.Queen | Pieces.White:
                    return "q";

                case Pieces.King:
                case Pieces.King | Pieces.Black:
                case Pieces.King | Pieces.White:
                    return "k";
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override IList<object> FormatCapture(BasicMove capture) =>
            this.FormatMove(capture);

        /// <inheritdoc />
        public override IList<object> FormatCastle(CastleMove castle) =>
            FormatUtilities.Build(
                this.FormatSquare(castle.GameState, castle.FromIndex),
                this.FormatSquare(castle.GameState, castle.ToIndex));

        /// <inheritdoc />
        public override IList<object> FormatEnPassantCapture(EnPassantCaptureMove enPassantCapture) =>
            this.FormatCapture(enPassantCapture);

        /// <inheritdoc />
        public override IList<object> FormatMove(BasicMove move) =>
            FormatUtilities.Build(
                this.FormatSquare(move.GameState, move.FromIndex),
                this.FormatSquare(move.GameState, move.ToIndex));

        /// <inheritdoc />
        public override IList<object> FormatPromotion(PromotionMove promotion) =>
            FormatUtilities.Build(
                this.FormatBasicMove(promotion),
                this.Format(promotion.PromotionPiece));

        /// <summary>
        /// Returns the string representation of the specified square.
        /// </summary>
        /// <param name="state">The game state.</param>
        /// <param name="index">The square to format.</param>
        /// <returns>The string representation of the square.</returns>
        protected virtual string FormatSquare(GameState state, int index)
        {
            state.Variant.GetCoordinates(index, out var x, out var y);
            return $"{(char)('a' + x)}{y + 1}";
        }
    }
}
