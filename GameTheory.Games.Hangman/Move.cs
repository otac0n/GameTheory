// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Hangman
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move in Hangman.
    /// </summary>
    public sealed class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="Hangman.GameState"/> that this move is based on.</param>
        /// <param name="guess">The letter being guessed.</param>
        public Move(GameState state, char guess)
        {
            this.GameState = state ?? throw new ArgumentNullException(nameof(state));
            this.PlayerToken = state.Players[0];
            this.Guess = guess;
        }

        /// <inheritdoc />
        public IList<object> FormatTokens => new object[] { this.Guess };

        /// <summary>
        /// Gets the letter being guessed.
        /// </summary>
        public char Guess { get; }

        /// <inheritdoc />
        public bool IsDeterministic => true;

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        internal GameState GameState { get; }

        /// <inheritdoc />
        public int CompareTo(Move other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            return this.Guess.CompareTo(other.Guess);
        }

        /// <inheritdoc />
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal GameState Apply(GameState state)
        {
            return state;
        }
    }
}
