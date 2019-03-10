// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Moves
{
    /// <summary>
    /// Represents a move where a <see cref="Pieces.Pawn">pawn</see> moves two spaces to an open square.
    /// </summary>
    public class TwoSquareMove : BasicMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwoSquareMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="fromIndex">The index of the square from which the <see cref="Pieces.Pawn">pawn</see> is moving.</param>
        /// <param name="toIndex">The index of the square to which the <see cref="Pieces.Pawn">pawn</see> is moving.</param>
        /// <param name="enPassantIndex">The index of the square made available for en passant capture.</param>
        public TwoSquareMove(GameState state, int fromIndex, int toIndex, int enPassantIndex)
            : base(state, fromIndex, toIndex)
        {
            this.EnPassantIndex = enPassantIndex;
        }

        /// <summary>
        /// Gets the index of the square made available for en passant capture.
        /// </summary>
        public int EnPassantIndex { get; }

        /// <inheritdoc />
        protected override GameState ApplyImpl(GameState state)
        {
            var board = state.Board;
            board[this.ToIndex] = state[this.FromIndex];
            board[this.FromIndex] = Pieces.None;
            return state.With(
                activeColor: state.ActiveColor == Pieces.White ? Pieces.Black : Pieces.White,
                plyCountClock: 0,
                board: board,
                enPassantIndex: this.EnPassantIndex);
        }
    }
}
