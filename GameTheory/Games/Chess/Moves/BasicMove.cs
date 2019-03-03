// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Moves
{
    using System.Threading;

    /// <summary>
    /// Represents a move where a <see cref="Pieces">chessman</see> moves to an open square or a square occupied by an opposing pice.
    /// </summary>
    public class BasicMove : Move
    {
        private GameState resultingState;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="fromIndex">The index of the square from which the <see cref="Pieces">chessman</see> is moving.</param>
        /// <param name="toIndex">The index of the square to which the <see cref="Pieces">chessman</see> is moving.</param>
        public BasicMove(GameState state, int fromIndex, int toIndex)
            : base(state)
        {
            this.FromIndex = fromIndex;
            this.ToIndex = toIndex;
        }

        /// <summary>
        /// Gets the index of the square from which the <see cref="Pieces">chessman</see> is moving.
        /// </summary>
        public int FromIndex { get; }

        /// <summary>
        /// Gets the index of the square to which the <see cref="Pieces">chessman</see> is moving.
        /// </summary>
        public int ToIndex { get; }

        /// <inheritdoc />
        internal sealed override GameState Apply(GameState state)
        {
            if (this.resultingState == null)
            {
                state = this.ApplyImpl(state);
                Interlocked.CompareExchange(ref this.resultingState, state, null);
            }

            return this.resultingState;
        }

        /// <summary>
        /// Implementations may override this method to provide custom application logic.
        /// </summary>
        /// <param name="state">The source game state.</param>
        /// <returns>The resuting game state.</returns>
        protected virtual GameState ApplyImpl(GameState state)
        {
            var castling = (state.Board[this.FromIndex] & PieceMasks.Piece) == Pieces.King
                ? GameState.RemoveCastling(state.Castling, state.ActiveColor)
                : GameState.RemoveCastling(state.Castling, this.FromIndex, this.ToIndex);
            return state.With(
                    activeColor: state.ActiveColor == Pieces.White ? Pieces.Black : Pieces.White,
                    plyCountClock: (state.Board[this.FromIndex] & PieceMasks.Piece) == Pieces.Pawn ? 0 : state.PlyCountClock + 1,
                    castling: castling,
                    board: state.Board
                        .SetItem(this.ToIndex, state.Board[this.FromIndex])
                        .SetItem(this.FromIndex, Pieces.None));
        }
    }
}
