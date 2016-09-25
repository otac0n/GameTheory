// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;
    using System.Linq;

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
        public override string ToString() => $"Take {string.Join(",", this.Tokens)}";

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var stacks = state.Tokens.Keys.Where(t => t != Token.GoldJoker);

            ////TODO: Yield moves that are combinations of up to three colors.

            foreach (var stack in stacks)
            {
                if (state.Tokens[stack] >= 4)
                {
                    yield return new TakeTokensMove(state, EnumCollection<Token>.Empty.Add(stack, 2));
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var tokens = state.Tokens;
            var pInventory = state.Inventory[state.ActivePlayer];
            var pTokens = pInventory.Tokens;

            tokens = tokens.RemoveRange(this.Tokens);
            pTokens = pTokens.AddRange(this.Tokens);

            return base.Apply(
                state.With(
                    tokens: tokens,
                    inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                        tokens: pTokens))));
        }
    }
}
