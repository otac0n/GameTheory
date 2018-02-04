// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents the current state in a game of Mancala.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="binsPerSide">The number of bins on each side of the board.</param>
        /// <param name="initialStonesPerBin">The number of stones initially in each bin.</param>
        public GameState(int binsPerSide = 6, int initialStonesPerBin = 4)
        {
            this.Players = ImmutableArray.Create(new PlayerToken(), new PlayerToken());
            this.ActivePlayer = this.Players[0];
            this.Phase = Phase.Play;
            this.BinsPerSide = binsPerSide;
            var bins = Enumerable.Repeat(initialStonesPerBin, binsPerSide);
            var side = Enumerable.Concat(bins, Enumerable.Repeat(0, 1));
            this.Board = ImmutableArray.CreateRange(Enumerable.Concat(side, side));
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            PlayerToken activePlayer,
            Phase phase,
            ImmutableArray<int> board)
        {
            this.Players = players;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.BinsPerSide = board.Length / 2 - 1;
            this.Board = board;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <summary>
        /// Gets the number of bins per side.
        /// </summary>
        public int BinsPerSide { get; }

        /// <summary>
        /// Gets the board.
        /// </summary>
        public ImmutableArray<int> Board { get; }

        /// <summary>
        /// Gets the current phase of the game.
        /// </summary>
        public Phase Phase { get; }

        /// <inheritdoc />
        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

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
                (comp = CompareUtilities.CompareValueLists(this.Board, state.Board)) != 0)
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

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.Players[0].GetHashCode());
            HashUtilities.Combine(ref hash, this.Players[1].GetHashCode());

            for (var i = 0; i < this.Board.Length; i++)
            {
                HashUtilities.Combine(ref hash, this.Board[i]);
            }

            return hash;
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
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
            playerToken == this.Players[0] ? 0 :
            playerToken == this.Players[1] ? this.BinsPerSide + 1 :
            throw new ArgumentOutOfRangeException(nameof(playerToken));

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
                throw new InvalidOperationException();
            }

            return move.Apply(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var r = new Func<IEnumerable<int>, string>(bins => string.Join(" ", bins.Select(b => $"[{this.Board[b],2}]")));
            return $"{r(this.GetPlayerIndexes(this.Players[1]).Reverse())} [  ]\n[  ] {r(this.GetPlayerIndexes(this.Players[0]))}";
        }

        internal GameState With(
            PlayerToken activePlayer = null,
            Phase? phase = null,
            ImmutableArray<int>? board = null)
        {
            return new GameState(
                this.Players,
                activePlayer ?? this.ActivePlayer,
                phase ?? this.Phase,
                board ?? this.Board);
        }
    }
}
