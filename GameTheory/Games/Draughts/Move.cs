// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using GameTheory.Games.Draughts.Moves;

    /// <summary>
    /// Represents a move in <see cref="Draughts.GameState">Draughts</see>.
    /// </summary>
    public abstract class Move : IMove
    {
        internal Move(GameState state)
        {
            this.GameState = state;
            this.PlayerToken = state.ActivePlayer;
        }

        /// <inheritdoc/>
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc/>
        public bool IsDeterministic => true;

        /// <inheritdoc/>
        public PlayerToken PlayerToken { get; }

        internal GameState GameState { get; }

        internal virtual GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;

            if (state.Phase == Phase.RemovePiece)
            {
                state = state.With(
                    phase: Phase.Play,
                    maxMovePieceIndexes: ImmutableSortedSet<int>.Empty);
            }
            else if (!CaptureMove.CapturesRemaining(state))
            {
                var variant = state.Variant;
                var board = state.Board;
                var playerIndex = state.Players.IndexOf(activePlayer);

                if (!variant.CrownOnEntry)
                {
                    var promoteRank = (variant.Height - 1) * playerIndex;
                    var playerColor = (Piece)(1 << playerIndex);
                    for (var x = 0; x < variant.Width; x++)
                    {
                        var index = variant.GetIndexOf(x, promoteRank);
                        if (index != -1)
                        {
                            var square = board[index];
                            if (square.HasFlag(playerColor) && !square.HasFlag(Piece.Crowned))
                            {
                                board = board
                                    .SetItem(index, square | Piece.Crowned);
                            }
                        }
                    }
                }

                for (var i = 0; i < board.Length; i++)
                {
                    if (board[i].HasFlag(Piece.Captured))
                    {
                        board = board.SetItem(i, Piece.None);
                    }
                }

                if (state.OpponentMayRemovePiece)
                {
                    state = state.With(
                        phase: Phase.RemovePiece,
                        opponentMayRemovePiece: false);
                }
                else
                {
                    state = state.With(
                        phase: Phase.Play,
                        maxMovePieceIndexes: ImmutableSortedSet<int>.Empty);
                }

                state = state.With(
                    activePlayer: state.Players[1 - playerIndex],
                    lastCapturingIndex: default(Maybe<int>),
                    board: board);
            }

            return state;
        }
    }
}
