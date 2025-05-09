﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to choose tokens to discard.
    /// </summary>
    public sealed class DiscardTokensMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscardTokensMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="tokens">The tokens to discard.</param>
        public DiscardTokensMove(GameState state, EnumCollection<Token> tokens)
            : base(state)
        {
            this.Tokens = tokens;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.DiscardTokens, this.Tokens);

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the tokens to discard.
        /// </summary>
        public EnumCollection<Token> Tokens { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is DiscardTokensMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.Tokens.CompareTo(move.Tokens)) != 0)
                {
                    return comp;
                }

                return 0;
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<DiscardTokensMove> GenerateMoves(GameState state)
        {
            var toDiscard = state.Inventory[state.ActivePlayer].Tokens.Count - GameState.TokenLimit;
            foreach (var discardTokens in state.Inventory[state.ActivePlayer].Tokens.Combinations(toDiscard))
            {
                yield return new DiscardTokensMove(state, discardTokens);
            }
        }

        internal static bool ShouldTransitionToPhase(GameState state)
        {
            if (state.Phase == Phase.Play)
            {
                return state.Inventory[state.ActivePlayer].Tokens.Count - GameState.TokenLimit > 0;
            }

            return false;
        }

        internal override GameState Apply(GameState state)
        {
            var tokens = state.Tokens;
            var pInventory = state.Inventory[state.ActivePlayer];
            var pTokens = pInventory.Tokens;

            pTokens = pTokens.RemoveRange(this.Tokens);
            tokens = tokens.AddRange(this.Tokens);

            return base.Apply(state.With(
                tokens: tokens,
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    tokens: pTokens))));
        }
    }
}
