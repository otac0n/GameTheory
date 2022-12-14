// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Moves
{
    /// <summary>
    /// Represents a move where a <see cref="Pieces.Pawn">pawn</see> captures another pawn en passant.
    /// </summary>
    public class EnPassantCaptureMove : BasicMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnPassantCaptureMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="fromIndex">The index of the square from which the <see cref="Pieces.Pawn">pawn</see> is moving.</param>
        /// <param name="toIndex">The index of the square to which the <see cref="Pieces.Pawn">pawn</see> is moving.</param>
        /// <param name="capturedIndex">The index of the <see cref="Pieces.Pawn">pawn</see> captured en passant.</param>
        public EnPassantCaptureMove(GameState state, int fromIndex, int toIndex, int capturedIndex)
            : base(state, fromIndex, toIndex)
        {
            this.CapturedIndex = capturedIndex;
        }

        /// <summary>
        /// Gets the index of the <see cref="Pieces.Pawn">pawn</see> captured en passant.
        /// </summary>
        public int CapturedIndex { get; }

        /// <inheritdoc />
        protected override GameState ApplyImpl(GameState state)
        {
            var board = state.Board;
            board[this.CapturedIndex] = Pieces.None;
            board[this.ToIndex] = state[this.FromIndex];
            board[this.FromIndex] = Pieces.None;
            return state.With(
                activeColor: state.ActiveColor == Pieces.White ? Pieces.Black : Pieces.White,
                plyCountClock: 0,
                board: board);
        }
    }
}
