// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to choose and remove a card from the challenging player's hand.
    /// </summary>
    public sealed class DiscardCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscardCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="card">The card to discard.</param>
        public DiscardCardMove(GameState state, Card card)
            : base(state)
        {
            this.Card = card;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Discard ", this.Card };

        /// <summary>
        /// Gets the index of the presented card to remove.
        /// </summary>
        public Card Card { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is DiscardCardMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.Card.CompareTo(move.Card)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Players, move.GameState.Players)) != 0)
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

        internal static IEnumerable<DiscardCardMove> GenerateMoves(GameState state)
        {
            var activePlayerInventory = state.Inventory[state.ActivePlayer];
            if (activePlayerInventory.Revealed[Card.Skull] > 0)
            {
                var cards = activePlayerInventory.Hand.AddRange(activePlayerInventory.Revealed);
                foreach (var card in cards.Keys)
                {
                    yield return new DiscardCardMove(state, card);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory;

            var activePlayerInventory = inventory[state.ActivePlayer];
            var cards = activePlayerInventory.Hand.AddRange(activePlayerInventory.Revealed);
            activePlayerInventory = activePlayerInventory.With(
                bid: cards.Count == 1 ? Inventory.PassingBid : 0,
                hand: cards.Remove(this.Card),
                revealed: EnumCollection<Card>.Empty,
                discards: activePlayerInventory.Discards.Add(this.Card));

            inventory = inventory.SetItem(
                state.ActivePlayer,
                activePlayerInventory);

            inventory = inventory.SetItems(
                from kvp in inventory
                let playerInventory = kvp.Value
                where playerInventory.Revealed.Count > 0 || playerInventory.Stack.Count > 0
                let newHand = playerInventory.Hand.AddRange(playerInventory.Revealed).AddRange(playerInventory.Stack)
                select new KeyValuePair<PlayerToken, Inventory>(kvp.Key, playerInventory.With(
                    bid: 0,
                    hand: newHand,
                    revealed: EnumCollection<Card>.Empty,
                    stack: ImmutableList<Card>.Empty)));

            state = state.With(
                phase: activePlayerInventory.Bid > Inventory.PassingBid
                    ? Phase.AddingCards
                    : inventory.Count(i => i.Value.Hand.Count > 0) > 1
                        ? Phase.ChooseStartingPlayer
                        : Phase.End,
                inventory: inventory);

            return base.Apply(state);
        }
    }
}
