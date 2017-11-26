// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to take tokens from the board.
    /// </summary>
    public sealed class TakeTokensMove : Move
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

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Take ", this.Tokens };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the tokens to be taken.
        /// </summary>
        public EnumCollection<Token> Tokens { get; }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var stacks = state.Tokens.Keys.Where(t => t != Token.GoldJoker).ToArray();

            for (var i = 0; i < stacks.Length; i++)
            {
                var firstOnly = EnumCollection<Token>.Empty.Add(stacks[i]);
                yield return new TakeTokensMove(state, firstOnly);

                for (var j = i + 1; j < stacks.Length; j++)
                {
                    var secondAndFirst = firstOnly.Add(stacks[j]);
                    yield return new TakeTokensMove(state, secondAndFirst);

                    for (var k = j + 1; k < stacks.Length; k++)
                    {
                        yield return new TakeTokensMove(state, secondAndFirst.Add(stacks[k]));
                    }
                }
            }

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
