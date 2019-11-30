// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Hangman
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents the current state in a game of Hangman.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        private static readonly string[] WordList = Resources.WordsAlpha.Split(new[] { '\r', '\n' });

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        public GameState()
        {
            this.Players = ImmutableArray.Create(new PlayerToken());
            this.IncorrectGuessLimit = 6;
            this.Word = WordList.Pick();
            this.Guesses = ImmutableHashSet<char>.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        /// <param name="word">The word being guessed.</param>
        public GameState(
            [PasswordPropertyText]
            string word)
        {
            this.Players = ImmutableArray.Create(new PlayerToken());
            this.IncorrectGuessLimit = 6;
            this.Word = string.IsNullOrEmpty(word) ? WordList.Pick() : word;
            this.Guesses = ImmutableHashSet<char>.Empty;
        }

        private GameState(ImmutableArray<PlayerToken> players, int incorrectGuessLimit, string word, ImmutableHashSet<char> guesses)
        {
            this.Players = players;
            this.IncorrectGuessLimit = incorrectGuessLimit;
            this.Word = word;
            this.Guesses = guesses;
        }

        /// <summary>
        /// Gets the guesses that have been chosen.
        /// </summary>
        public ImmutableHashSet<char> Guesses { get; }

        /// <summary>
        /// Gets the number of incorrect guesses.
        /// </summary>
        public int IncorrectGuesses => this.Guesses.Except(this.Word).Count;

        /// <summary>
        /// Gets the number of incorrect guesses that ends the game.
        /// </summary>
        public int IncorrectGuessLimit { get; }

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

        /// <inheritdoc />
        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the word being guessed.
        /// </summary>
        public string Word { get; }

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

            if ((comp = string.CompareOrdinal(this.Word, state.Word)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0)
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
            return !this.Word.All(this.Guesses.Contains) && this.IncorrectGuesses < this.IncorrectGuessLimit
                ? Enumerable
                    .Range('a', 'z' - 'a' + 1)
                    .Select(n => (char)n)
                    .Where(c => !this.Guesses.Contains(c))
                    .Select(c => new Move(this, c))
                    .ToImmutableList()
                : ImmutableList<Move>.Empty;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.Word.GetHashCode());

            for (var i = 0; i < this.Players.Length; i++)
            {
                HashUtilities.Combine(ref hash, this.Players[i].GetHashCode());
            }

            return hash;
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <inheritdoc />
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            return new[]
            {
                new GameState(
                    this.Players,
                    this.IncorrectGuessLimit,
                    Regex.Replace(this.Word, ".", match => this.Guesses.Contains(match.Value[0]) ? match.Value : "_"),
                    this.Guesses),
            };
        }

        /// <inheritdoc />
        public IReadOnlyList<PlayerToken> GetWinners() =>
            this.Word.All(this.Guesses.Contains)
                ? this.Players
                : ImmutableArray<PlayerToken>.Empty;

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

            if (this.Guesses.Contains(move.Guess))
            {
                throw new ArgumentOutOfRangeException(nameof(move));
            }

            return move.Apply(this);
        }

        internal GameState With(ImmutableHashSet<char> guesses = null)
        {
            return new GameState(
                this.Players,
                this.IncorrectGuessLimit,
                this.Word,
                guesses: guesses ?? this.Guesses);
        }
    }
}
