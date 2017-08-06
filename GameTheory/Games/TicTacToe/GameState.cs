// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TicTacToe
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Implements the game of Tic-tac-toe.
    /// </summary>
    public class GameState : IGameState<Move>
    {
        private const int Size = 3;
        private readonly ImmutableList<PlayerToken> players;
        private readonly ImmutableList<PlayerToken> winners;
        private readonly PlayerToken winningPlayer;
        private PlayerToken[,] field;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        public GameState()
            : this(ImmutableList.Create(new PlayerToken(), new PlayerToken()), new PlayerToken[Size, Size])
        {
        }

        private GameState(ImmutableList<PlayerToken> players, PlayerToken[,] field)
        {
            this.players = players;
            this.field = field;

            var p0 = this.players[0];
            var p1 = this.players[1];
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

            this.ActivePlayer = p0count <= p1count ? p0 : p1;

            if ((field[0, 0] == field[1, 1] && field[1, 1] == field[2, 2]) ||
                (field[0, 2] == field[1, 1] && field[1, 1] == field[2, 0]))
            {
                this.winningPlayer = field[1, 1];
            }
            else
            {
                for (var x = 0; x < Size; x++)
                {
                    if (field[x, 1] != null && field[x, 0] == field[x, 1] && field[x, 1] == field[x, 2])
                    {
                        this.winningPlayer = field[x, 1];
                        break;
                    }
                }

                if (this.winningPlayer == null)
                {
                    for (var y = 0; y < Size; y++)
                    {
                        if (field[1, y] != null && field[0, y] == field[1, y] && field[1, y] == field[2, y])
                        {
                            this.winningPlayer = field[1, y];
                            break;
                        }
                    }
                }
            }

            this.winners = this.winningPlayer == null
                ? ImmutableList<PlayerToken>.Empty
                : ImmutableList.Create(this.winningPlayer);
        }

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> representing the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <inheritdoc />
        public IReadOnlyList<PlayerToken> Players => this.players;

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> of the player who marked the specified spot.
        /// </summary>
        /// <param name="x">The x coordinate of the spot.</param>
        /// <param name="y">The y coordinate of the spot.</param>
        /// <returns>The <see cref="PlayerToken"/> of the player who marked the specified spot, or <c>null</c> if no player has marked the spot.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public PlayerToken this[int x, int y] => this.field[x, y];

        /// <inheritdoc />
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            if (this.winningPlayer != null)
            {
                return ImmutableList<Move>.Empty;
            }

            var moves = ImmutableList.CreateBuilder<Move>();

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (this.field[x, y] == null)
                    {
                        moves.Add(new Move(this.ActivePlayer, x, y));
                    }
                }
            }

            return moves.ToImmutable();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners() => this.winners;

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <inheritdoc />
        public IGameState<Move> MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (move.PlayerToken != this.ActivePlayer ||
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

            newField[move.X, move.Y] = move.PlayerToken;

            return new GameState(this.players, newField);
        }

        /// <inheritdoc />
        public IGameState<Move> GetView(PlayerToken playerToken) => this;

        /// <inheritdoc />
        public override string ToString()
        {
            var c = new Func<int, int, string>((x, y) => this[x, y] == null ? " " : this[x, y] == this.players[0] ? "X" : "O");
            return $"{c(0, 0)}|{c(1, 0)}|{c(2, 0)}\n-----\n{c(0, 1)}|{c(1, 1)}|{c(2, 1)}\n-----\n{c(0, 2)}|{c(1, 2)}|{c(2, 2)}";
        }

        /// <inheritdoc/>
        public int CompareTo(IGameState<Move> other)
        {
            if (other == this)
            {
                return 0;
            }

            var state = other as GameState;
            if (state == null)
            {
                return 1;
            }

            int comp;

            if (this.players != state.players)
            {
                if ((comp = this.players[0].CompareTo(state.players[0])) != 0 ||
                    (comp = this.players[1].CompareTo(state.players[1])) != 0)
                {
                    return comp;
                }
            }

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var left = this.field[x, y];
                    var right = state.field[x, y];
                    if (left != right)
                    {
                        return left == null ? -1 : left.CompareTo(right);
                    }
                }
            }

            return 0;
        }
    }
}
