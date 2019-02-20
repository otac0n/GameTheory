// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Notation
{
    using System.Collections.Generic;
    using GameTheory.Games.Chess.Moves;

    public class LongAlgebraicNotation : NotationSystem
    {
        /// <inheritdoc />
        public override string Format(Pieces piece)
        {
            switch (piece)
            {
                case Pieces.Pawn | Pieces.Black:
                    return "p";

                case Pieces.Pawn | Pieces.White:
                case Pieces.Pawn:
                    return "P";

                case Pieces.Knight | Pieces.Black:
                    return "n";

                case Pieces.Knight | Pieces.White:
                case Pieces.Knight:
                    return "N";

                case Pieces.Bishop | Pieces.Black:
                    return "b";

                case Pieces.Bishop | Pieces.White:
                case Pieces.Bishop:
                    return "B";

                case Pieces.Rook | Pieces.Black:
                    return "r";

                case Pieces.Rook | Pieces.White:
                case Pieces.Rook:
                    return "R";

                case Pieces.Queen | Pieces.Black:
                    return "q";

                case Pieces.Queen | Pieces.White:
                case Pieces.Queen:
                    return "Q";

                case Pieces.King | Pieces.Black:
                    return "k";

                case Pieces.King | Pieces.White:
                case Pieces.King:
                    return "K";
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override IList<object> FormatCapture(BasicMove capture) =>
            FormatUtilities.Build(
                FormatMovingPiece(capture),
                this.FormatSquare(capture.GameState, capture.FromIndex),
                "x",
                this.FormatSquare(capture.GameState, capture.ToIndex));

        /// <inheritdoc />
        public override IList<object> FormatCastle(CastleMove castle) =>
            new[] { (castle.CastlingSide & PieceMasks.Piece) == Pieces.King ? "O-O" : "O-O-O" };

        /// <inheritdoc />
        public override IList<object> FormatEnPassantCapture(EnPassantCaptureMove enPassantCapture) =>
            FormatUtilities.Build(
                this.FormatCapture(enPassantCapture),
                "e.p.");

        /// <inheritdoc />
        public override IList<object> FormatMove(BasicMove capture) =>
            FormatUtilities.Build(
                FormatMovingPiece(capture),
                this.FormatSquare(capture.GameState, capture.FromIndex),
                "-",
                this.FormatSquare(capture.GameState, capture.ToIndex));

        /// <inheritdoc />
        public override IList<object> FormatPromotion(PromotionMove promotion) =>
            FormatUtilities.Build(
                this.FormatBasicMove(promotion),
                "=",
                promotion.PromotionPiece & PieceMasks.Piece);

        private static object FormatMovingPiece(BasicMove capture) =>
            FormatUtilities.If(
                (capture.GameState.Board[capture.FromIndex] & PieceMasks.Piece) != Pieces.Pawn,
                () => capture.GameState.Board[capture.FromIndex] & PieceMasks.Piece);

        private string FormatSquare(GameState gameState, int index)
        {
            gameState.Variant.GetCoordinates(index, out var x, out var y);
            return $"{(char)('a' + x)}{y + 1}";
        }
    }
}
