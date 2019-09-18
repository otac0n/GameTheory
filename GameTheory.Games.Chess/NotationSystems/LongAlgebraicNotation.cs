// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.NotationSystems
{
    using System.Collections.Generic;
    using GameTheory.Games.Chess.Moves;

    /// <summary>
    /// A coordinate based notation for representing moves.
    /// </summary>
    public class LongAlgebraicNotation : AlgebraicNotation
    {
        /// <inheritdoc />
        public override IList<object> FormatCapture(BasicMove capture) =>
            FormatUtilities.Build(
                FormatMovingPiece(capture),
                this.FormatSquare(capture.GameState, capture.FromIndex),
                "x",
                this.FormatSquare(capture.GameState, capture.ToIndex));

        /// <inheritdoc />
        public override IList<object> FormatMove(BasicMove move) =>
            FormatUtilities.Build(
                FormatMovingPiece(move),
                this.FormatSquare(move.GameState, move.FromIndex),
                "-",
                this.FormatSquare(move.GameState, move.ToIndex));

        private static object FormatMovingPiece(BasicMove move) =>
            FormatUtilities.If(
                (move.GameState[move.FromIndex] & PieceMasks.Piece) != Pieces.Pawn,
                () => move.GameState[move.FromIndex] & PieceMasks.Piece);
    }
}
