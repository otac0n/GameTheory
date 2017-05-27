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
        private readonly PlayerToken activePlayer;
        private readonly ImmutableArray<int> board;
        private readonly ImmutableList<PlayerToken> players;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="binsPerSide">The number of bins on each side of the board.</param>
        /// <param name="initialStonesPerBin">The number of stones initially in each bin.</param>
        public GameState(int binsPerSide = 6, int initialStonesPerBin = 4)
        {
            this.players = ImmutableList.Create(new PlayerToken(), new PlayerToken());
            this.activePlayer = this.players[0];
            var bins = Enumerable.Repeat(initialStonesPerBin, binsPerSide);
            var side = Enumerable.Concat(bins, Enumerable.Repeat(0, 1));
            this.board = ImmutableArray.CreateRange(Enumerable.Concat(side, side));
        }

        private GameState(
            ImmutableList<PlayerToken> players,
            PlayerToken activePlayer,
            ImmutableArray<int> board)
        {
            this.players = players;
            this.activePlayer = activePlayer;
            this.board = board;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer => this.activePlayer;

        /// <summary>
        /// Gets the board.
        /// </summary>
        public ImmutableArray<int> Board => this.board;

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableList<PlayerToken> Players => this.players;

        /// <summary>
        /// Gets the number of bins per side.
        /// </summary>
        public int BinsPerSide => this.Board.Length / 2 - 1;

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken playerToken)
        {
            var moves = ImmutableList.CreateBuilder<Move>();

            if (playerToken == this.activePlayer)
            {
                var playerOffset = this.GetPlayerIndexOffset(this.activePlayer);
                var binsPerSide = this.BinsPerSide;
                for (var i = 0; i < binsPerSide; i++)
                {
                    var index = i + playerOffset;
                    if (this.board[index] > 0)
                    {
                        moves.Add(new Move(this, index));
                    }
                }
            }

            return moves.ToImmutable();
        }

        /// <summary>
        /// Gets the score of the specified player.
        /// </summary>
        /// <param name="playerToken">The player whose score should be calculated.</param>
        /// <returns>The specified player's score.</returns>
        public int GetScore(PlayerToken playerToken)
        {
            return this.GetPlayerIndexes(playerToken).Sum(i => this.board[i]);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            return this.players
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
            var r = new Func<IEnumerable<int>, string>(bins => string.Join(" ", bins.Select(b => $"[{this.board[b], 2}]")));
            return $"{r(this.GetPlayerIndexes(this.players[1]).Reverse())} [  ]\n[  ] {r(this.GetPlayerIndexes(this.players[0]))}";
        }

        /// <summary>
        /// Enumerates the indexes for the specified player.
        /// </summary>
        /// <param name="playerToken">The player whose bin indexes should be returned.</param>
        /// <returns>An enumerable collection of indexes.</returns>
        public IEnumerable<int> GetPlayerIndexes(PlayerToken playerToken) =>
            Enumerable.Range(this.GetPlayerIndexOffset(playerToken), this.board.Length / 2);

        /// <summary>
        /// Gets the starting index for the specified player.
        /// </summary>
        /// <param name="playerToken">The player whose starting index should be returned.</param>
        /// <returns>The lowest index representing a bin owned by this player.</returns>
        public int GetPlayerIndexOffset(PlayerToken playerToken) =>
            this.players.IndexOf(playerToken) * this.board.Length / 2;

        internal GameState With(
            PlayerToken activePlayer = null,
            ImmutableArray<int>? board = null)
        {
            return new GameState(
                this.players,
                activePlayer ?? this.activePlayer,
                board ?? this.board);
        }
    }
}
