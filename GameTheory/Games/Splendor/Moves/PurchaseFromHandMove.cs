// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to purchase a development card from the player's hand.
    /// </summary>
    public class PurchaseFromHandMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseFromHandMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="cost">The effective token cost of this card, including substituted jokers and bonuses.</param>
        /// <param name="index">The index in the hand of the card to purchase.</param>
        public PurchaseFromHandMove(GameState state, EnumCollection<Token> cost, int index)
            : base(state)
        {
            this.Cost = cost;
            this.Index = index;
        }

        /// <summary>
        /// Gets the development card to purchase.
        /// </summary>
        public DevelopmentCard Card => this.State.Inventory[this.State.ActivePlayer].Hand[this.Index];

        /// <summary>
        /// Gets the effective token cost of this card, including substituted jokers and bonuses.
        /// </summary>
        public EnumCollection<Token> Cost { get; }

        /// <summary>
        /// Gets the index in the development track of the card to purchase.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => $"Purchase {this.Card} for {(this.Cost.Count > 0 ? this.Cost.ToString() : "free")}";

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var tokens = state.Inventory[state.ActivePlayer].Tokens;
            var jokerCount = tokens[Token.GoldJoker];
            tokens = tokens.RemoveAll(Token.GoldJoker);

            var bonus = state.GetBonus(state.ActivePlayer);

            var hand = state.Inventory[state.ActivePlayer].Hand;
            for (var i = 0; i < hand.Count; i++)
            {
                var cost = hand[i].Cost.RemoveRange(bonus);
                foreach (var tokenCost in PurchaseFromBoardMove.GetAffordableTokenCosts(tokens, jokerCount, cost))
                {
                    yield return new PurchaseFromHandMove(state, tokenCost, i);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var tokens = state.Tokens;
            var pInventory = state.Inventory[state.ActivePlayer];
            var pDevelopmentCards = pInventory.DevelopmentCards;
            var pTokens = pInventory.Tokens;
            var pHand = state.Inventory[state.ActivePlayer].Hand;
            var card = pHand[this.Index];

            tokens = tokens.AddRange(this.Cost);
            pTokens = pTokens.RemoveRange(this.Cost);

            pDevelopmentCards = pDevelopmentCards.Add(card);
            pHand = pHand.RemoveAt(this.Index);

            return base.Apply(state.With(
                tokens: tokens,
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    developmentCards: pDevelopmentCards,
                    tokens: pTokens,
                    hand: pHand))));
        }
    }
}
