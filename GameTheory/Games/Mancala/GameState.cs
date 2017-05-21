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
        internal const int BinsOnASide = 6;
        private const int InitialStonesPerBin = 4;
        private static readonly ImmutableArray<int> InitialBoard;
        private readonly PlayerToken activePlayer;
        private readonly ImmutableArray<int> board;
        private readonly ImmutableList<PlayerToken> players;

        static GameState()
        {
            var bins = Enumerable.Repeat(InitialStonesPerBin, BinsOnASide);
            var side = Enumerable.Concat(bins, Enumerable.Repeat(0, 1));
            InitialBoard = ImmutableArray.CreateRange(Enumerable.Concat(side, side));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        public GameState()
        {
            this.players = ImmutableList.Create(new PlayerToken(), new PlayerToken());
            this.activePlayer = this.players[0];
            this.board = InitialBoard;
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

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken playerToken)
        {
            var moves = ImmutableList.CreateBuilder<Move>();

            if (playerToken == this.activePlayer)
            {
                foreach (var i in this.GetPlayerIndexes(this.activePlayer).Take(BinsOnASide))
                {
                    if (this.board[i] > 0)
                    {
                        moves.Add(new Move(this, i));
                    }
                }
            }

            return moves.ToImmutable();
        }

        /// <summary>
        /// Gets the score of the specified player.
        /// </summary>
        /// <param name="player">The player whose score should be calculated.</param>
        /// <returns>The specified player's score.</returns>
        public int GetScore(PlayerToken player)
        {
            return this.GetPlayerIndexes(player).Sum(i => this.board[i]);
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
        IGameState<Move> IGameState<Move>.GetView(PlayerToken playerToken) => this;

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

        internal IEnumerable<int> GetPlayerIndexes(PlayerToken player)
        {
            var playerIndex = this.players.IndexOf(player);
            var startingIndex = playerIndex * (BinsOnASide + 1);
            var indexes = Enumerable.Range(startingIndex, BinsOnASide + 1);
            return indexes;
        }

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
