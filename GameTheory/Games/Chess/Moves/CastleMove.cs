// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Moves
{
    using System.Threading;

    /// <summary>
    /// Represents a move to castle the king.
    /// </summary>
    public class CastleMove : Move
    {
        private GameState resultingState;

        /// <summary>
        /// Initializes a new instance of the <see cref="CastleMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="castlingSide">The side to which the king will be castling.</param>
        /// <param name="fromIndex">The index of the square from which the <see cref="Pieces.King"/> is moving.</param>
        /// <param name="toIndex">The index of the square to which the <see cref="Pieces.King"/> is moving.</param>
        /// <param name="rookFromIndex">The index of the square from which the <see cref="Pieces.Rook"/> is moving.</param>
        /// <param name="rookToIndex">The index of the square to which the <see cref="Pieces.Rook"/> is moving.</param>
        public CastleMove(GameState state, Pieces castlingSide, int fromIndex, int toIndex, int rookFromIndex, int rookToIndex)
            : base(state)
        {
            this.CastlingSide = castlingSide;
            this.FromIndex = fromIndex;
            this.ToIndex = toIndex;
            this.RookFromIndex = rookFromIndex;
            this.RookToIndex = rookToIndex;
        }

        /// <summary>
        /// Gets the side to which the king will be castling.
        /// </summary>
        public Pieces CastlingSide { get; }

        /// <summary>
        /// Gets the index of the square from which the <see cref="Pieces.King"/> is moving.
        /// </summary>
        public int FromIndex { get; }

        /// <summary>
        /// Gets the index of the square from which the <see cref="Pieces.Rook"/> is moving.
        /// </summary>
        public int RookFromIndex { get; }

        /// <summary>
        /// Gets the index of the square to which the <see cref="Pieces.Rook"/> is moving.
        /// </summary>
        public int RookToIndex { get; }

        /// <summary>
        /// Gets the index of the square to which the <see cref="Pieces.King"/> is moving.
        /// </summary>
        public int ToIndex { get; }

        internal override GameState Apply(GameState state)
        {
            if (this.resultingState == null)
            {
                var newBoard = state.Board.ToBuilder();
                newBoard[this.ToIndex] = state.Board[this.FromIndex];
                newBoard[this.FromIndex] = Pieces.None;
                newBoard[this.RookToIndex] = state.Board[this.RookFromIndex];
                newBoard[this.RookFromIndex] = Pieces.None;
                state = state.With(
                    activeColor: state.ActiveColor == Pieces.White ? Pieces.Black : Pieces.White,
                    plyCountClock: state.PlyCountClock + 1,
                    castling: GameState.RemoveCastling(state.Castling, state.ActiveColor),
                    board: newBoard.ToImmutable());

                Interlocked.CompareExchange(ref this.resultingState, state, null);
            }

            return this.resultingState;
        }
    }
}
