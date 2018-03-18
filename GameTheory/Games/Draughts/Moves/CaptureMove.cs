// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to capture a piece.
    /// </summary>
    public sealed class CaptureMove : BasicMove
    {
        private CaptureMove(GameState state, int fromIndex, int toIndex, int captureIndex)
            : base(state, fromIndex, toIndex)
        {
            this.CaptureIndex = captureIndex;
        }

        /// <summary>
        /// Gets the index of the square that the piece is being captured.
        /// </summary>
        public int CaptureIndex { get; }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { (this.FromIndex + 1), "x", (this.ToIndex + 1) };

        internal static bool CapturesRemaining(GameState state)
        {
            return state.LastCapturingIndex.HasValue && CaptureMove.GenerateMoves(state).Any();
        }

        internal static new IEnumerable<CaptureMove> GenerateMoves(GameState state)
        {
            var variant = state.Variant;
            var board = state.Board;
            var playerIndex = state.Players.IndexOf(state.ActivePlayer);
            var opponentIndex = 1 - playerIndex;
            var playerColor = (Pieces)(1 << playerIndex);
            var opponentColor = (Pieces)(1 << opponentIndex);
            var forwardDirection = playerIndex * 2 - 1;

            int i, count;
            if (state.LastCapturingIndex.HasValue)
            {
                i = state.LastCapturingIndex.Value;
                count = i + 1;
            }
            else
            {
                i = 0;
                count = board.Length;
            }

            for (; i < count; i++)
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
                            var u = 1;
                            var captureIndex = -1;

                            while (true)
                            {
                                var capX = x + dx * u;
                                var capY = y + dy * u;
                                u++;

                                var capIndex = variant.GetIndexOf(capX, capY);
                                if (capIndex < 0)
                                {
                                    break;
                                }

                                var captureSquare = board[capIndex];
                                if ((captureSquare & opponentColor) == opponentColor && (captureSquare & Pieces.Captured) != Pieces.Captured && (variant.MenCanCaptureKings || (captureSquare & Pieces.Crowned) != Pieces.Crowned))
                                {
                                    captureIndex = capIndex;
                                    break;
                                }
                                else if (captureSquare == Pieces.None && crowned && variant.FlyingKings)
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (captureIndex < 0)
                            {
                                continue;
                            }

                            for (; ; u++)
                            {
                                var toX = x + dx * u;
                                var toY = y + dy * u;

                                var toIndex = variant.GetIndexOf(toX, toY);
                                if (toIndex < 0)
                                {
                                    break;
                                }

                                var toSquare = board[toIndex];
                                if (toSquare == Pieces.None)
                                {
                                    yield return new CaptureMove(state, i, toIndex, captureIndex);

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

                        if (!crowned && !variant.MenCaptureBackwards)
                        {
                            break;
                        }
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var board = state.Board;

            state = state.With(
                board: board.SetItem(this.CaptureIndex, board[this.CaptureIndex] | Pieces.Captured),
                lastCapturingIndex: this.ToIndex);

            return base.Apply(state);
        }
    }
}
