// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents the current state of a game of Mancala.
    /// </summary>
    public class GameState : IGameState<Move>
    {
        private readonly PlayerToken player0;
        private readonly PlayerToken player1;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="binsPerSide">The number of bins on each side of the board.</param>
        /// <param name="initialStonesPerBin">The number of stones initially in each bin.</param>
        public GameState(int binsPerSide = 6, int initialStonesPerBin = 4)
        {
            this.Players = ImmutableList.Create(new PlayerToken(), new PlayerToken());
            this.player0 = this.Players[0];
            this.player1 = this.Players[1];
            this.ActivePlayer = this.player0;
            this.BinsPerSide = binsPerSide;
            var bins = Enumerable.Repeat(initialStonesPerBin, binsPerSide);
            var side = Enumerable.Concat(bins, Enumerable.Repeat(0, 1));
            this.Board = ImmutableArray.CreateRange(Enumerable.Concat(side, side));
        }

        private GameState(
            ImmutableList<PlayerToken> players,
            PlayerToken activePlayer,
            ImmutableArray<int> board)
        {
            this.Players = players;
            this.player0 = this.Players[0];
            this.player1 = this.Players[1];
            this.ActivePlayer = activePlayer;
            this.BinsPerSide = board.Length / 2 - 1;
            this.Board = board;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <summary>
        /// Gets the board.
        /// </summary>
        public ImmutableArray<int> Board { get; }

        /// <inheritdoc />
        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableList<PlayerToken> Players { get; }

        /// <summary>
        /// Gets the number of bins per side.
        /// </summary>
        public int BinsPerSide { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves()
        {
            int bins = this.BinsPerSide;
            var moves = new Move[bins];

            var playerOffset = this.GetPlayerIndexOffset(this.ActivePlayer);
            var b = 0;
            for (var i = 0; i < bins; i++)
            {
                var index = i + playerOffset;
                if (this.Board[index] > 0)
                {
                    moves[b++] = new Move(this, index);
                }
            }

            Array.Resize(ref moves, b);
            return moves;
        }

        /// <summary>
        /// Gets the score of the specified player.
        /// </summary>
        /// <param name="playerToken">The player whose score should be calculated.</param>
        /// <returns>The specified player's score.</returns>
        public int GetScore(PlayerToken playerToken)
        {
            return this.GetPlayerIndexes(playerToken).Sum(i => this.Board[i]);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            return this.Players
                .GroupBy(p => this.GetScore(p))
                .OrderByDescending(g => g.Key)
                .First()
                .ToImmutableList();
        }

        /// <inheritdoc />
        IGameState<Move> IGameState<Move>.MakeMove(Move move)
        {
            return this.MakeMove(move);
        }

        /// <inheritdoc />
        public IGameState<Move> GetView(PlayerToken playerToken) => this;

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

            if (move.State != this)
            {
                throw new InvalidOperationException();
            }

            return move.Apply(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var r = new Func<IEnumerable<int>, string>(bins => string.Join(" ", bins.Select(b => $"[{this.Board[b], 2}]")));
            return $"{r(this.GetPlayerIndexes(this.Players[1]).Reverse())} [  ]\n[  ] {r(this.GetPlayerIndexes(this.Players[0]))}";
        }

        /// <summary>
        /// Enumerates the indexes for the specified player.
        /// </summary>
        /// <param name="playerToken">The player whose bin indexes should be returned.</param>
        /// <returns>An enumerable collection of indexes.</returns>
        public IEnumerable<int> GetPlayerIndexes(PlayerToken playerToken) =>
            Enumerable.Range(this.GetPlayerIndexOffset(playerToken), this.BinsPerSide + 1);

        /// <summary>
        /// Gets the starting index for the specified player.
        /// </summary>
        /// <param name="playerToken">The player whose starting index should be returned.</param>
        /// <returns>The lowest index representing a bin owned by this player.</returns>
        public int GetPlayerIndexOffset(PlayerToken playerToken) =>
            playerToken == this.player0 ? 0 :
            playerToken == this.player1 ? this.BinsPerSide + 1 :
            throw new ArgumentOutOfRangeException(nameof(playerToken));

        internal GameState With(
            PlayerToken activePlayer = null,
            ImmutableArray<int>? board = null)
        {
            return new GameState(
                this.Players,
                activePlayer ?? this.ActivePlayer,
                board ?? this.Board);
        }
    }
}
