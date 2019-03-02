// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Moves
{
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
            : base(state, fromIndex, toIndex)
        {
            this.PromotionPiece = promotionPiece;
        }

        /// <summary>
        /// Gets the piece to which the pawn will be promoted.
        /// </summary>
        public Pieces PromotionPiece { get; }

        /// <inheritdoc />
        protected override GameState ApplyImpl(GameState state) =>
            state.With(
                activeColor: state.ActiveColor == Pieces.White ? Pieces.Black : Pieces.White,
                plyCountClock: 0,
                castling: GameState.RemoveCastling(state.Castling, this.ToIndex),
                board: state.Board
                    .SetItem(this.ToIndex, this.PromotionPiece)
                    .SetItem(this.FromIndex, Pieces.None));
    }
}
