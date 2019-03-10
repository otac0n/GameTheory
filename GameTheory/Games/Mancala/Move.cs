// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Mancala.
    /// </summary>
    public sealed class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="Mancala.GameState"/> that this move is based on.</param>
        /// <param name="bin">The index of the bin.</param>
        internal Move(GameState state, int bin)
        {
            this.GameState = state ?? throw new ArgumentNullException(nameof(state));
            this.Bin = bin;
        }

        /// <summary>
        /// Gets the index of the bin.
        /// </summary>
        public int Bin { get; }

        /// <inheritdoc />
        public IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.PickUpStones, this.GameState[this.Bin], this.Bin);

        /// <inheritdoc />
        public bool IsDeterministic => true;

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        public PlayerToken PlayerToken => this.GameState.ActivePlayer;

        internal GameState GameState { get; }

        /// <inheritdoc />
        public override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal GameState Apply(GameState state)
        {
            var activePlayerIndex = state.ActivePlayerIndex;
            var binsPerSide = state.BinsPerSide;
            var phase = state.Phase;

            var captureMin = state.GetPlayerIndexOffset(activePlayerIndex);
            var mancala = captureMin + binsPerSide;

            var otherPlayerIndex = 1 - activePlayerIndex;
            var othersBinsMin = state.GetPlayerIndexOffset(otherPlayerIndex);
            var otherMancala = othersBinsMin + binsPerSide;

            var bin = this.Bin;
            var board = state.Board;

            var count = board[bin];
            var lastValue = board[bin] = 0;

            while (count > 0)
            {
                do
                {
                    bin += 1;
                    bin %= board.Length;
                }
                while (bin == otherMancala);

                board[bin] = lastValue = board[bin] + 1;
                count -= 1;
            }

            if (bin != mancala)
            {
                if (lastValue == 1 && bin >= captureMin && bin < mancala)
                {
                    var captureIndex = binsPerSide - (bin - binsPerSide);
                    if (board[captureIndex] > 0)
                    {
                        board[mancala] += lastValue + board[captureIndex];
                        board[bin] = 0;
                        board[captureIndex] = 0;
                    }
                }

                activePlayerIndex = otherPlayerIndex;
            }

            var playerBins = Enumerable.Range(captureMin, binsPerSide);
            var othersBins = Enumerable.Range(othersBinsMin, binsPerSide);
            if (playerBins.All(i => board[i] == 0))
            {
                phase = Phase.End;
                foreach (var i in othersBins)
                {
                    board[otherMancala] += board[i];
                    board[i] = 0;
                }
            }
            else if (othersBins.All(i => board[i] == 0))
            {
                phase = Phase.End;
                foreach (var i in playerBins)
                {
                    board[mancala] += board[i];
                    board[i] = 0;
                }
            }

            return state.With(
                activePlayerIndex: activePlayerIndex,
                phase: phase,
                board: board);
        }
    }
}
