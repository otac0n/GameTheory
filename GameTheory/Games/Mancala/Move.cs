﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala
{
    using System;
    using System.Linq;

    /// <summary>
    /// Represents a move in Mancala.
    /// </summary>
    public sealed class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="bin">The index of the bin.</param>
        internal Move(GameState state, int bin)
        {
            this.State = state ?? throw new ArgumentNullException(nameof(state));
            this.PlayerToken = state.ActivePlayer;
            this.Bin = bin;
        }

        /// <summary>
        /// Gets the index of the bin.
        /// </summary>
        public int Bin { get; }

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc />
        public bool IsDeterministic => true;

        internal GameState State { get; }

        /// <inheritdoc />
        public override string ToString() => $"Pick up {this.State.Board[this.Bin]} stones from ({this.Bin})";

        internal GameState Apply(GameState state)
        {
            var binsPerSide = state.BinsPerSide;
            var captureMin = state.GetPlayerIndexOffset(this.PlayerToken);
            var mancala = captureMin + binsPerSide;

            var otherPlayer = state.Players.Except(this.PlayerToken).Single();
            var othersBinsMin = state.GetPlayerIndexOffset(otherPlayer);
            var otherMancala = othersBinsMin + binsPerSide;

            var bin = this.Bin;
            var board = state.Board;

            var count = board[bin];
            board = board.SetItem(bin, 0);

            while (count > 0)
            {
                do
                {
                    bin += 1;
                    bin %= board.Length;
                }
                while (bin == otherMancala);

                board = board.SetItem(bin, board[bin] + 1);
                count -= 1;
            }

            PlayerToken activePlayer;
            if (bin == mancala)
            {
                activePlayer = this.PlayerToken;
            }
            else
            {
                if (board[bin] == 1 && bin >= captureMin && bin < mancala)
                {
                    var captureIndex = binsPerSide - (bin - binsPerSide);
                    if (board[captureIndex] > 0)
                    {
                        board = board
                            .SetItem(mancala, board[mancala] + board[bin] + board[captureIndex])
                            .SetItem(bin, 0)
                            .SetItem(captureIndex, 0);
                    }
                }

                activePlayer = otherPlayer;
            }

            var playerBins = Enumerable.Range(captureMin, binsPerSide);
            var othersBins = Enumerable.Range(othersBinsMin, binsPerSide);
            if (playerBins.All(i => board[i] == 0))
            {
                foreach (var i in othersBins)
                {
                    board = board
                        .SetItem(otherMancala, board[otherMancala] + board[i])
                        .SetItem(i, 0);
                }
            }
            else if (othersBins.All(i => board[i] == 0))
            {
                foreach (var i in playerBins)
                {
                    board = board
                        .SetItem(mancala, board[mancala] + board[i])
                        .SetItem(i, 0);
                }
            }

            return state.With(
                board: board,
                activePlayer: activePlayer);
        }
    }
}
