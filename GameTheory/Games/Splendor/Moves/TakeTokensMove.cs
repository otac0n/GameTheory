// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    /// <summary>
    /// Represents a move to take tokens from the board.
    /// </summary>
    public class TakeTokensMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TakeTokensMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="tokens">The tokens to be taken.</param>
        public TakeTokensMove(GameState state, EnumCollection<Token> tokens)
            : base(state)
        {
            this.Tokens = tokens;
        }

        /// <summary>
        /// Gets the tokens to be taken.
        /// </summary>
        public EnumCollection<Token> Tokens { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Take {string.Join(",", this.Tokens)}";
        }

        internal override GameState Apply(GameState state)
        {
            return base.Apply(state);
        }
    }
}
