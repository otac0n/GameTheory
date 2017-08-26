// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;
    using GameTheory.Games.CenturySpiceRoad.MerchantCards;

    /// <summary>
    /// Represents a move to initiate the upgrade of spices.
    /// </summary>
    public class UpgradeCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the card to play.</param>
        public UpgradeCardMove(GameState state, int index)
            : base(state)
        {
            this.Index = index;
        }

        /// <summary>
        /// Gets the index of the card to play.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc/>
        public override string ToString() => $"Play [{this.State.Inventory[this.PlayerToken].Hand[this.Index]}]";

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var inventory = state.Inventory[state.ActivePlayer];
            if (inventory.Caravan.Count > inventory.Caravan[Spice.Cinnamon])
            {
                var hand = inventory.Hand;
                for (var i = 0; i < hand.Count; i++)
                {
                    if (hand[i] is UpgradeCard)
                    {
                        yield return new UpgradeCardMove(state, i);
                    }
                }
            }
        }

        /// <inheritdoc />
        internal override GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var pInventory = state.Inventory[activePlayer];
            var card = (UpgradeCard)pInventory.Hand[this.Index];

            pInventory = pInventory.With(
                hand: pInventory.Hand.RemoveAt(this.Index),
                playedCards: pInventory.PlayedCards.Add(card));

            state = state.With(
                inventory: state.Inventory.SetItem(activePlayer, pInventory),
                upgradesRemaining: card.UpgradeLevel,
                phase: Phase.Upgrade);

            return base.Apply(state);
        }
    }
}
