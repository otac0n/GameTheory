// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Moves
{
    using System.Linq;

    /// <summary>
    /// Represents a move to castle the king.
    /// </summary>
    public class CastleMove : Move
    {
        private readonly GameState resultingState;

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
            this.resultingState = Move.Advance(
                state.With(
                    plyCountClock: state.PlyCountClock + 1,
                    castling: state.Castling.RemoveRange(state.Castling.Keys.Where(k => (k & PieceMasks.Colors) == state.ActiveColor)),
                    board: state.Board
                        .SetItem(toIndex, state.Board[fromIndex])
                        .SetItem(fromIndex, Pieces.None)
                        .SetItem(rookToIndex, state.Board[rookFromIndex])
                        .SetItem(rookFromIndex, Pieces.None)));
        }

        /// <summary>
        /// Gets the side to which the king will be castling.
        /// </summary>
        public Pieces CastlingSide { get; }

        internal override GameState Apply(GameState state) => this.resultingState;
    }
}
