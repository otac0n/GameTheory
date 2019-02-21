// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Moves
{
    using System.Linq;

    /// <summary>
    /// Represents a move where a <see cref="Pieces.Pawn">pawn</see> is promoted.
    /// </summary>
    public class PromotionMove : BasicMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromotionMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="fromIndex">The index of the square from which the <see cref="Pieces.Pawn">pawn</see> is moving.</param>
        /// <param name="toIndex">The index of the square to which the <see cref="Pieces.Pawn">pawn</see> is moving.</param>
        /// <param name="promotionPiece">The piece to which the pawn will be promoted.</param>
        public PromotionMove(GameState state, int fromIndex, int toIndex, Pieces promotionPiece)
            : base(
                state,
                fromIndex,
                toIndex,
                Move.Advance(state.With(
                    plyCountClock: 0,
                    castling: state.Castling.RemoveRange(state.Castling.Keys.Where(k => state.Castling[k] == toIndex)),
                    board: state.Board
                        .SetItem(toIndex, promotionPiece)
                        .SetItem(fromIndex, Pieces.None))))
        {
            this.PromotionPiece = promotionPiece;
        }

        /// <summary>
        /// Gets the piece to which the pawn will be promoted.
        /// </summary>
        public Pieces PromotionPiece { get; }
    }
}
