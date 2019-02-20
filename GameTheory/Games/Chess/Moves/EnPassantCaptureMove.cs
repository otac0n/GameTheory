using System.Collections.Generic;

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
            : base(
                state,
                fromIndex,
                toIndex,
                Move.Advance(state.With(
                    plyCountClock: 0,
                    board: state.Board
                        .SetItem(toIndex, state.Board[fromIndex])
                        .SetItem(fromIndex, Pieces.None)
                        .SetItem(capturedIndex, Pieces.None))))
        {
            this.CapturedIndex = capturedIndex;
        }

        /// <summary>
        /// Gets the index of the <see cref="Pieces.Pawn">pawn</see> captured en passant.
        /// </summary>
        public int CapturedIndex { get; }
    }
}
