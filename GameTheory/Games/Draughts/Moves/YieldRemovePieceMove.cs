// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to opt out of the removal of a piece.
    /// </summary>
    public sealed class YieldRemovePieceMove : Move
    {
        private YieldRemovePieceMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { "Remove nothing" };

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            if (state.Phase == Phase.RemovePiece)
            {
                yield return new YieldRemovePieceMove(state);
            }
        }
    }
}
