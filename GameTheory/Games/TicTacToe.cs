// -----------------------------------------------------------------------
// <copyright file="TicTacToe.cs" company="(none)">
//   Copyright © 2014 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implements the game of Tic-tac-toe.
    /// </summary>
    public class TicTacToe : IGameState<TicTacToe.Move>
    {
        private const int Size = 3;
        private static readonly IReadOnlyCollection<Move> EmptyMoveList = new List<Move>(0).AsReadOnly();
        private static readonly IReadOnlyCollection<PlayerToken> EmptyWinnerList = new List<PlayerToken>(0).AsReadOnly();
        private readonly PlayerToken activePlayer;
        private readonly Lazy<IReadOnlyCollection<Move>> availableMoves;
        private readonly Lazy<IReadOnlyCollection<PlayerToken>> winners;
        private readonly PlayerToken winningPlayer;
        private PlayerToken[,] field;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicTacToe"/> class in the starting position.
        /// </summary>
        public TicTacToe()
            : this(new List<PlayerToken> { new PlayerToken(), new PlayerToken() }.AsReadOnly(), new PlayerToken[Size, Size])
        {
        }

        private TicTacToe(IReadOnlyList<PlayerToken> players, PlayerToken[,] field)
        {
            this.Players = players;
            this.field = field;

            var p0 = this.Players[0];
            var p1 = this.Players[1];
            var p0count = 0;
            var p1count = 0;

            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    var px = this.field[x, y];

                    if (px == p0)
                    {
                        p0count++;
                    }
                    else if (px == p1)
                    {
                        p1count++;
                    }
                }
            }

            this.activePlayer = p0count <= p1count ? p0 : p1;

            if ((field[0, 0] == field[1, 1] && field[1, 1] == field[2, 2]) ||
                (field[0, 2] == field[1, 1] && field[1, 1] == field[2, 0]))
            {
                this.winningPlayer = field[1, 1];
            }
            else
            {
                for (var x = 0; x < Size; x++)
                {
                    if (field[x, 0] == field[x, 1] && field[x, 1] == field[x, 2])
                    {
                        this.winningPlayer = field[x, 1];
                        break;
                    }
                }

                if (this.winningPlayer == null)
                {
                    for (var y = 0; y < Size; y++)
                    {
                        if (field[0, y] == field[1, y] && field[1, y] == field[2, y])
                        {
                            this.winningPlayer = field[1, y];
                            break;
                        }
                    }
                }
            }

            this.availableMoves = new Lazy<IReadOnlyCollection<Move>>(this.GetAvailableMoves);
            this.winners = new Lazy<IReadOnlyCollection<PlayerToken>>(() => new List<PlayerToken> { this.winningPlayer }.AsReadOnly());
        }

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> representing the active player.
        /// </summary>
        public PlayerToken ActivePlayer
        {
            get { return this.activePlayer; }
        }

        /// <inheritdoc />
        public IReadOnlyList<PlayerToken> Players { get; private set; }

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> of the player who marked the specified spot.
        /// </summary>
        /// <param name="x">The x coordinate of the spot.</param>
        /// <param name="y">The y coordinate of the spot.</param>
        /// <returns>The <see cref="PlayerToken"/> of the player who marked the specified spot, or <c>null</c> if no player has marked the spot.</returns>
        public PlayerToken this[int x, int y]
        {
            get
            {
                return this.field[x, y];
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken player)
        {
            if (player != this.activePlayer || this.winningPlayer != null)
            {
                return EmptyMoveList;
            }

            return this.availableMoves.Value;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.winningPlayer == null)
            {
                return EmptyWinnerList;
            }

            return this.winners.Value;
        }

        /// <inheritdoc />
        public IGameState<Move> MakeMove(Move move)
        {
            if (move.Player != this.activePlayer ||
                move.X < 0 ||
                move.X >= Size ||
                move.Y < 0 ||
                move.Y >= Size ||
                this.field[move.X, move.Y] != null)
            {
                throw new ArgumentOutOfRangeException("move");
            }

            var newField = new PlayerToken[Size, Size];
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    newField[x, y] = this.field[x, y];
                }
            }

            newField[move.X, move.Y] = move.Player;

            return new TicTacToe(this.Players, newField);
        }

        private IReadOnlyCollection<Move> GetAvailableMoves()
        {
            var moves = new List<Move>(9);

            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    if (this.field[x, y] == null)
                    {
                        moves.Add(new Move(this.activePlayer, x, y));
                    }
                }
            }

            return moves.AsReadOnly();
        }

        /// <summary>
        /// Represents a move in Tic-tac-toe.
        /// </summary>
        public struct Move : IMove
        {
            private readonly PlayerToken player;
            private readonly int x;
            private readonly int y;

            /// <summary>
            /// Initializes a new instance of the <see cref="Move"/> struct.
            /// </summary>
            /// <param name="player">The player who may make this move.</param>
            /// <param name="x">The x coordinate of the spot on which the move will me made.</param>
            /// <param name="y">The y coordinate of the spot on which the move will me made.</param>
            public Move(PlayerToken player, int x, int y)
            {
                this.player = player;
                this.x = x;
                this.y = y;
            }

            /// <inheritdoc />
            public PlayerToken Player
            {
                get { return this.player; }
            }

            /// <summary>
            /// Gets the x coordinate of the spot on which the move will me made.
            /// </summary>
            public int X
            {
                get { return this.x; }
            }

            /// <summary>
            /// Gets the y coordinate of the spot on which the move will me made.
            /// </summary>
            public int Y
            {
                get { return this.y; }
            }
        }
    }
}
