// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TwentyFortyEight
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GameTheory.Games.TwentyFortyEight.Moves;
    using Random = Random;

    /// <summary>
    /// Indicates the current turn.
    /// </summary>
    public enum Turn : byte
    {
        /// <summary>
        /// Indicates the first player's turn.
        /// </summary>
        Player,

        /// <summary>
        /// Indicates the second player's turn.
        /// </summary>
        Computer,
    }

    /// <summary>
    /// Implements the game of 2048.
    /// </summary>
    public class GameState : IGameState<Move>
    {
        /// <summary>
        /// Indicates the width and height of the playing area.
        /// </summary>
        public const int Size = 4;

        /// <summary>
        /// The smaller value that can be added by the computer player.
        /// </summary>
        public const byte SmallValue = 1;

        /// <summary>
        /// The larger value that can be added by the computer player.
        /// </summary>
        public const byte LargeValue = 2;

        private const int WinThreshold = 11;
        private const double SmallValueWeight = 0.9;
        private readonly IReadOnlyList<PlayerToken> players;
        private readonly byte[,] field;
        private readonly Turn turn;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        /// <param name="players">The number of players playing.</param>
        public GameState(int players = 1)
            : this(Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableArray(), Turn.Computer, new byte[Size, Size])
        {
            if (players < 1 || players > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            if (players == 1)
            {
                RandomComputerMove(this.field);
                RandomComputerMove(this.field);
                this.turn = Turn.Player;
            }
        }

        private GameState(IReadOnlyList<PlayerToken> players, Turn turn, byte[,] field)
        {
            this.players = players;
            this.field = field;
            this.turn = turn;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PlayerToken> Players => this.players;

        /// <summary>
        /// Gets the value of the specified spot on the playing area.
        /// </summary>
        /// <param name="x">The x coordinate of the spot.</param>
        /// <param name="y">The y coordinate of the spot.</param>
        /// <returns>The <see cref="PlayerToken"/> of the player who marked the specified spot, or <c>null</c> if no player has marked the spot.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public byte this[int x, int y] => this.field[x, y];

        /// <inheritdoc/>
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            if (this.turn == 0)
            {
                return PlayerMove.GetMoves(this).ToArray();
            }
            else
            {
                return ComputerMove.GetMoves(this).ToArray();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            if (move.IsDeterministic)
            {
                yield return Weighted.Create(this.MakeMove(move), 1);
                yield break;
            }

            var state = move.Apply(this);
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (state.field[x, y] == 0)
                    {
                        var field = new byte[Size, Size];
                        Array.Copy(state.field, field, field.Length);
                        field[x, y] = SmallValue;
                        yield return Weighted.Create(state.With(turn: Turn.Player, field: field), SmallValueWeight);

                        field = new byte[Size, Size];
                        Array.Copy(state.field, field, field.Length);
                        field[x, y] = LargeValue;
                        yield return Weighted.Create(state.With(turn: Turn.Player, field: field), 1 - SmallValueWeight);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IGameState<Move> GetView(PlayerToken playerToken) => this;

        /// <inheritdoc/>
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (this.field[x, y] >= WinThreshold)
                    {
                        return new[] { this.players[0] };
                    }
                }
            }

            if (this.players.Count == 2 && this.turn == Turn.Player)
            {
                if (!PlayerMove.GetMoves(this).Any())
                {
                    return new[] { this.players[1] };
                }
            }

            return ImmutableArray<PlayerToken>.Empty;
        }

        /// <inheritdoc/>
        public IGameState<Move> MakeMove(Move move)
        {
            if (this.CompareTo(move.State) != 0)
            {
                throw new InvalidOperationException();
            }

            var state = move.Apply(this);

            if (this.players.Count == 1 && state.turn == Turn.Computer)
            {
                var field = new byte[Size, Size];
                Array.Copy(state.field, field, field.Length);
                RandomComputerMove(field);
                state = state.With(turn: Turn.Player, field: field);
            }

            return state;
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

            if ((comp = this.turn.CompareTo(state.turn)) != 0)
            {
                return comp;
            }

            if (this.players != state.players)
            {
                if ((comp = this.players.Count.CompareTo(state.players.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.players.Count; i++)
                {
                    if ((comp = this.players[i].CompareTo(state.players[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if ((comp = this.field[x, y].CompareTo(state.field[x, y])) != 0)
                    {
                        return comp;
                    }
                }
            }

            return 0;
        }

        internal GameState With(
            Turn? turn = null,
            byte[,] field = null)
        {
            return new GameState(
                this.Players,
                turn ?? this.turn,
                field ?? this.field);
        }

        private static void RandomComputerMove(byte[,] field)
        {
            var found = 0;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (field[x, y] == 0)
                    {
                        found++;
                    }
                }
            }

            var picked = Random.Instance.Next(found);

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (field[x, y] == 0)
                    {
                        if (picked == 0)
                        {
                            field[x, y] = Random.Instance.NextDouble() < SmallValueWeight ? SmallValue : LargeValue;
                            return;
                        }

                        picked--;
                    }
                }
            }
        }
    }
}
