﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        public int Bin { get; private set; }

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        public PlayerToken PlayerToken { get; private set; }

        internal GameState State { get; private set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Pick up {this.State.Board[this.Bin]} stones from ({this.Bin})";
        }

        internal GameState Apply(GameState state)
        {
            var mancala = state.GetPlayerIndexes(this.PlayerToken).Last();
            var captureSpots = new HashSet<int>(state.GetPlayerIndexes(this.PlayerToken).Take(GameState.BinsOnASide));
            var otherPlayer = state.Players.Except(this.PlayerToken).Single();
            var otherMancala = state.GetPlayerIndexes(otherPlayer).Last();
            var othersBins = new HashSet<int>(state.GetPlayerIndexes(otherPlayer).Take(GameState.BinsOnASide));

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
                if (board[bin] == 1 && captureSpots.Contains(bin))
                {
                    var captureIndex = GameState.BinsOnASide - (bin - GameState.BinsOnASide);
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

            if (captureSpots.All(i => board[i] == 0))
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
                foreach (var i in captureSpots)
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
