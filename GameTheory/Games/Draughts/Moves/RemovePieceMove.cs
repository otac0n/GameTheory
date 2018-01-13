// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to remove a piece.
    /// </summary>
    public sealed class RemovePieceMove : Move
    {
        private RemovePieceMove(GameState state, int removeIndex)
            : base(state)
        {
            this.RemoveIndex = removeIndex;
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { "Remove ", (this.RemoveIndex + 1) };

        /// <summary>
        /// Gets the index of the square of the peice that will be removed.
        /// </summary>
        public int RemoveIndex { get; }

        internal static IEnumerable<RemovePieceMove> GenerateMoves(GameState state)
        {
            if (state.Phase == Phase.RemovePiece)
            {
                foreach (var index in state.MaxMovePieceIndices)
                {
                    yield return new RemovePieceMove(state, index);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var board = state.Board;
            state = state.With(
                board: board.SetItem(this.RemoveIndex, Piece.None));

            return base.Apply(state);
        }
    }
}
