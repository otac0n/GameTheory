// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Represents the current state in a game of Mancala.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        private readonly int[] board;

        /// <summary>
        /// The maximum number of supported bins per side.
        /// </summary>
        public const int MaxBinsPerSide = 10;

        /// <summary>
        /// The minimum number of supported bins per side.
        /// </summary>
        public const int MinBinsPerSide = 1;

        /// <summary>
        /// The maximum number of supported initial stones per bin.
        /// </summary>
        public const int MaxInitialStonesPerBin = 10;

        /// <summary>
        /// The minimum number of supported initial stones per bin.
        /// </summary>
        public const int MinInitialStonesPerBin = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="binsPerSide">The number of bins on each side of the board.</param>
        /// <param name="initialStonesPerBin">The number of stones initially in each bin.</param>
        public GameState([Range(MinBinsPerSide, MaxBinsPerSide)] int binsPerSide = 6, [Range(MinInitialStonesPerBin, MaxInitialStonesPerBin)] int initialStonesPerBin = 4)
        {
            if (binsPerSide < MinBinsPerSide || binsPerSide > MaxBinsPerSide)
            {
                throw new ArgumentOutOfRangeException(nameof(binsPerSide));
            }
            else if (initialStonesPerBin < MinInitialStonesPerBin || initialStonesPerBin > MaxInitialStonesPerBin)
            {
                throw new ArgumentOutOfRangeException(nameof(initialStonesPerBin));
            }

            this.Players = ImmutableArray.Create(new PlayerToken(), new PlayerToken());
            this.ActivePlayerIndex = 0;
            this.Phase = Phase.Play;
            this.BinsPerSide = binsPerSide;
            var bins = Enumerable.Repeat(initialStonesPerBin, binsPerSide);
            var side = Enumerable.Concat(bins, Enumerable.Repeat(0, 1));
            this.board = Enumerable.Concat(side, side).ToArray();
        }

        private GameState(
            IReadOnlyList<PlayerToken> players,
            int activePlayerIndex,
            Phase phase,
            int[] board)
        {
            this.Players = players;
            this.ActivePlayerIndex = activePlayerIndex;
            this.Phase = phase;
            this.BinsPerSide = board.Length / 2 - 1;
            this.board = board;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer => this.Players[this.ActivePlayerIndex];

        /// <summary>
        /// Gets the index of the active player.
        /// </summary>
        public int ActivePlayerIndex { get; }

        /// <summary>
        /// Gets the number of bins per side.
        /// </summary>
        public int BinsPerSide { get; }

        /// <summary>
        /// Gets a copy of the internal board.
        /// </summary>
        public int[] Board
        {
            get
            {
                var length = this.board.Length;
                var board = new int[length];
                Array.Copy(this.board, board, length);
                return board;
            }
        }

        /// <summary>
        /// Gets the current phase of the game.
        /// </summary>
        public Phase Phase { get; }

        /// <inheritdoc />
        public IReadOnlyList<PlayerToken> Players { get; }

        /// <summary>
        /// Gets the value of the specified bin.
        /// </summary>
        /// <param name="bin">The index of the bin.</param>
        /// <returns>The value of the specified bin.</returns>
        public int this[int bin] => this.board[bin];

        /// <inheritdoc/>
        public int CompareTo(IGameState<Move> other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            var state = other as GameState;
            if (object.ReferenceEquals(state, null))
            {
                return 1;
            }

            int comp;

            if ((comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0 ||
                (comp = CompareUtilities.CompareValueLists(this.board, state.board)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => this.CompareTo(obj as IGameState<Move>) == 0;

        /// <inheritdoc />
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            var bins = this.BinsPerSide;
            var moves = new Move[bins];

            var playerOffset = this.GetPlayerIndexOffset(this.ActivePlayerIndex);
            var b = 0;
            for (var i = 0; i < bins; i++)
            {
                var index = i + playerOffset;
                if (this.board[index] > 0)
                {
                    moves[b++] = new Move(this, index);
                }
            }

            Array.Resize(ref moves, b);
            return moves;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.ActivePlayerIndex);

            for (var i = 0; i < this.board.Length; i++)
            {
                HashUtilities.Combine(ref hash, this.board[i]);
            }

            return hash;
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move) =>
            new IWeighted<IGameState<Move>>[] { Weighted.Create(this.MakeMove(move), 1) };

        /// <summary>
        /// Enumerates the indexes for the specified player.
        /// </summary>
        /// <param name="playerIndex">The player whose bin indexes should be returned.</param>
        /// <returns>An enumerable collection of indexes.</returns>
        public IEnumerable<int> GetPlayerIndexes(int playerIndex) =>
            Enumerable.Range(this.GetPlayerIndexOffset(playerIndex), this.BinsPerSide + 1);

        /// <summary>
        /// Gets the starting index for the specified player.
        /// </summary>
        /// <param name="playerIndex">The player whose starting index should be returned.</param>
        /// <returns>The lowest index representing a bin owned by this player.</returns>
        public int GetPlayerIndexOffset(int playerIndex) => playerIndex * (this.BinsPerSide + 1);

        /// <summary>
        /// Gets the score of the specified player.
        /// </summary>
        /// <param name="playerToken">The player whose score should be calculated.</param>
        /// <returns>The specified player's score.</returns>
        public int GetScore(PlayerToken playerToken)
        {
            return this.GetPlayerIndexes(this.Players.IndexOf(playerToken)).Sum(i => this.board[i]);
        }

        /// <inheritdoc />
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            yield return this;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.Phase != Phase.End)
            {
                return ImmutableList<PlayerToken>.Empty;
            }

            return this.Players.AllMaxBy(p => this.GetScore(p)).ToImmutableList();
        }

        /// <inheritdoc />
        IGameState<Move> IGameState<Move>.MakeMove(Move move)
        {
            return this.MakeMove(move);
        }

        /// <summary>
        /// Applies the move to the current game state.
        /// </summary>
        /// <param name="move">The <see cref="Move"/> to apply.</param>
        /// <returns>The updated <see cref="GameState"/>.</returns>
        public GameState MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (this.CompareTo(move.GameState) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(move));
            }

            return move.Apply(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var r = new Func<IEnumerable<int>, string>(bins => string.Join(" ", bins.Select(b => $"[{this.board[b],2}]")));
            return $"{r(this.GetPlayerIndexes(1).Reverse())} [  ]\n[  ] {r(this.GetPlayerIndexes(0))}";
        }

        internal GameState With(
            int? activePlayerIndex = null,
            Phase? phase = null,
            int[] board = null)
        {
            return new GameState(
                this.Players,
                activePlayerIndex ?? this.ActivePlayerIndex,
                phase ?? this.Phase,
                board ?? this.board);
        }
    }
}
