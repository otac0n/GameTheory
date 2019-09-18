// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.NotationSystems
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.Chess.Moves;

    /// <summary>
    /// A reduced coordinate based notation for representing moves.
    /// </summary>
    public class AlgebraicNotation : NotationSystem
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
            (capture.GameState[capture.FromIndex] & PieceMasks.Piece) == Pieces.Pawn
            ? FormatUtilities.Build(
                ((char)('a' + capture.GameState.Variant.GetCoordinates(capture.FromIndex).X)).ToString(),
                "x",
                this.FormatSquare(capture.GameState, capture.ToIndex))
            : FormatUtilities.Build(
                FormatMovingPiece(capture),
                this.DisambuiguateSource(capture.GameState, capture.FromIndex, capture.ToIndex),
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
        public override IList<object> FormatMove(BasicMove move) =>
            (move.GameState[move.FromIndex] & PieceMasks.Piece) == Pieces.Pawn
                ? new[] { this.FormatSquare(move.GameState, move.ToIndex) }
                : FormatUtilities.Build(
                    FormatMovingPiece(move),
                    this.DisambuiguateSource(move.GameState, move.FromIndex, move.ToIndex),
                    this.FormatSquare(move.GameState, move.ToIndex));

        /// <inheritdoc />
        public override IList<object> FormatPromotion(PromotionMove promotion) =>
            FormatUtilities.Build(
                this.FormatBasicMove(promotion),
                "=",
                promotion.PromotionPiece & PieceMasks.Piece);

        /// <summary>
        /// Returns the string representation of the specified square.
        /// </summary>
        /// <param name="state">The game state.</param>
        /// <param name="index">The square to format.</param>
        /// <returns>The string representation of the square.</returns>
        protected string FormatSquare(GameState state, int index)
        {
            state.Variant.GetCoordinates(index, out var x, out var y);
            return $"{(char)('a' + x)}{y + 1}";
        }

        private static object FormatMovingPiece(BasicMove move) => move.GameState[move.FromIndex] & PieceMasks.Piece;

        private object DisambuiguateSource(GameState gameState, int fromIndex, int toIndex)
        {
            var ambiguousSources = gameState
                .GetAvailableMoves()
                .OfType<BasicMove>()
                .Where(m => m.FromIndex != fromIndex && m.ToIndex == toIndex && gameState[m.FromIndex] == gameState[fromIndex])
                .Select(m => gameState.Variant.GetCoordinates(m.FromIndex))
                .ToList();
            if (ambiguousSources.Count == 0)
            {
                return null;
            }

            gameState.Variant.GetCoordinates(fromIndex, out var x, out var y);
            if (ambiguousSources.All(a => a.X != x))
            {
                return ((char)('a' + x)).ToString();
            }
            else if (ambiguousSources.All(a => a.Y != y))
            {
                return (y + 1).ToString();
            }
            else
            {
                return $"{(char)('a' + x)}{y + 1}";
            }
        }
    }
}
