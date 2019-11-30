// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Hangman
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents the current state in a game of Hangman.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// All of the letters available to guess.
        /// </summary>
        public static readonly ImmutableArray<string> Letters = Enumerable.Range('a', 'z' - 'a' + 1).Select(n => ((char)n).ToString()).ToImmutableArray();

        private static readonly string[] WordList = Resources.WordsAlpha.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        private readonly string word;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        public GameState()
        {
            this.Players = ImmutableArray.Create(new PlayerToken());
            this.IncorrectGuessLimit = 6;
            this.word = WordList.Pick();
            this.Guesses = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        /// <param name="word">The word being guessed.</param>
        public GameState(
            [PasswordPropertyText(true)]
            string word)
        {
            this.Players = ImmutableArray.Create(new PlayerToken());
            this.IncorrectGuessLimit = 6;
            this.word = string.IsNullOrEmpty(word) ? WordList.Pick() : word;
            this.Guesses = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);
        }

        private GameState(ImmutableArray<PlayerToken> players, int incorrectGuessLimit, string word, ImmutableHashSet<string> guesses)
        {
            this.Players = players;
            this.IncorrectGuessLimit = incorrectGuessLimit;
            this.word = word;
            this.Guesses = guesses;
        }

        /// <summary>
        /// Gets the guesses that have been chosen.
        /// </summary>
        public ImmutableHashSet<string> Guesses { get; }

        /// <summary>
        /// Gets the number of incorrect guesses.
        /// </summary>
        public int IncorrectGuesses => this.Guesses.Except(this.word.Select(c => c.ToString())).Count;

        /// <summary>
        /// Gets the number of incorrect guesses that ends the game.
        /// </summary>
        public int IncorrectGuessLimit { get; }

        /// <summary>
        /// Gets a value indicating whether the puzzle is solved.
        /// </summary>
        public bool IsSolved => this.word.All(c => !GameState.IsLetter(c) || this.Guesses.Contains(c.ToString()));

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

        /// <inheritdoc />
        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the word being guessed.
        /// </summary>
        public string Word =>
            this.IncorrectGuesses >= this.IncorrectGuessLimit
                ? this.word
                : this.MaskWord();

        /// <summary>
        /// Is the character a letter available to guess?
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns><c>true</c>, if the character is a letter available to guess; <c>false</c>, otherwise.</returns>
        public static bool IsLetter(string c) => c.Length == 1 && GameState.IsLetter(c[0]);

        /// <summary>
        /// Is the character a letter available to guess?
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns><c>true</c>, if the character is a letter available to guess; <c>false</c>, otherwise.</returns>
        public static bool IsLetter(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');

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

            if ((comp = string.CompareOrdinal(this.word, state.word)) != 0 ||
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
            return this.IsSolved || this.IncorrectGuesses >= this.IncorrectGuessLimit
                ? ImmutableList<Move>.Empty
                : GameState.Letters
                    .Where(c => !this.Guesses.Contains(c))
                    .Select(c => new Move(this, c))
                    .ToImmutableList();
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.word.GetHashCode());

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
            var remaining = GameState.Letters
                .Where(c => !this.Guesses.Contains(c))
                .ToList();

            for (; maxStates > 0; maxStates--)
            {
                yield return new GameState(
                    this.Players,
                    this.IncorrectGuessLimit,
                    this.MaskWord(c => remaining.Pick()),
                    this.Guesses);
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<PlayerToken> GetWinners() =>
            this.IsSolved
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

        internal GameState With(ImmutableHashSet<string> guesses = null)
        {
            return new GameState(
                this.Players,
                this.IncorrectGuessLimit,
                this.word,
                guesses: guesses ?? this.Guesses);
        }

        private string MaskWord(Func<string, string> mask = null)
        {
            if (mask == null)
            {
                mask = _ => "_";
            }

            var builder = new StringBuilder(this.word);
            for (var i = builder.Length - 1; i >= 0; i--)
            {
                if (GameState.IsLetter(builder[i]))
                {
                    var c = builder[i].ToString();
                    if (!this.Guesses.Contains(c))
                    {
                        builder.Replace(c, mask(c), i, 1);
                    }
                }
            }

            return builder.ToString();
        }
    }
}
