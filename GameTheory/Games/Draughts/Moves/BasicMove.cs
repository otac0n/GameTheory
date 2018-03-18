// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the move of a piece.
    /// </summary>
    public class BasicMove : Move
    {
        internal BasicMove(GameState state, int fromIndex, int toIndex)
            : base(state)
        {
            this.FromIndex = fromIndex;
            this.ToIndex = toIndex;
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { (this.FromIndex + 1), "-", (this.ToIndex + 1) };

        /// <summary>
        /// Gets the index of the square that the piece is moving from.
        /// </summary>
        public int FromIndex { get; }

        /// <summary>
        /// Gets the index of the square that the piece is moving to.
        /// </summary>
        public int ToIndex { get; }

        internal static IEnumerable<BasicMove> GenerateMoves(GameState state)
        {
            if (state.LastCapturingIndex.HasValue)
            {
                yield break;
            }

            var variant = state.Variant;
            var board = state.Board;
            var playerIndex = state.Players.IndexOf(state.ActivePlayer);
            var playerColor = (Pieces)(1 << playerIndex);
            var forwardDirection = playerIndex * 2 - 1;

            var count = board.Length;
            for (var i = 0; i < count; i++)
            {
                var square = board[i];
                if ((square & playerColor) == playerColor)
                {
                    var crowned = (square & Pieces.Crowned) == Pieces.Crowned;
                    variant.GetCoordinates(i, out int x, out int y);

                    for (var f = 1; f >= -1; f -= 2)
                    {
                        var dy = f * forwardDirection;
                        for (var dx = -1; dx <= 1; dx += 2)
                        {
                            for (var u = 1; ; u++)
                            {
                                var toX = x + dx * u;
                                var toY = y + dy * u;

                                var toIndex = variant.GetIndexOf(toX, toY);
                                if (toIndex < 0)
                                {
                                    break;
                                }

                                if (board[toIndex] == Pieces.None)
                                {
                                    yield return new BasicMove(state, i, toIndex);

                                    if (!crowned || !variant.FlyingKings)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (!crowned)
                        {
                            break;
                        }
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var variant = state.Variant;
            var board = state.Board;
            board = board
                .SetItem(this.ToIndex, board[this.FromIndex])
                .SetItem(this.FromIndex, Pieces.None);

            if (variant.CrownOnEntry && (board[this.ToIndex] & Pieces.Crowned) != Pieces.Crowned)
            {
                var playerIndex = state.Players.IndexOf(state.ActivePlayer);
                var promoteRank = (variant.Height - 1) * playerIndex;
                variant.GetCoordinates(this.ToIndex, out int x, out int y);
                if (y == promoteRank)
                {
                    board = board
                        .SetItem(this.ToIndex, board[this.ToIndex] | Pieces.Crowned);
                }
            }

            if (variant.MovePriorityImpact == MovePriorityImpact.PieceRemoval)
            {
                var indexes = state.MaxMovePieceIndexes;
                if (indexes.Contains(this.FromIndex))
                {
                    state = state.With(
                        maxMovePieceIndexes: indexes.Remove(this.FromIndex).Add(this.ToIndex));
                }
            }

            state = state.With(
                board: board);

            return base.Apply(state);
        }
    }
}
