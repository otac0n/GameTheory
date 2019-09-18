// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Moves
{
    using System.Collections.Generic;
    using GameTheory.Games.SevenDragons.Cards;

    /// <summary>
    /// Represents a move to discard an action card instead of playing it.
    /// </summary>
    public sealed class DiscardCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscardCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="handIndex">The index of the card being discarded.</param>
        public DiscardCardMove(GameState state, int handIndex)
            : base(state)
        {
            this.HandIndex = handIndex;
        }

        /// <summary>
        /// Gets the index of the card being discarded.
        /// </summary>
        public int HandIndex { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens =>
            FormatUtilities.ParseStringFormat(Resources.DiscardActionCard, this.GameState.Inventories[this.GameState.InventoryMap[this.GameState.ActivePlayer]].Hand[this.HandIndex]);

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is DiscardCardMove drawCards)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.HandIndex.CompareTo(drawCards.HandIndex)) != 0)
                {
                    return comp;
                }

                var card = this.GameState.Inventories[this.GameState.InventoryMap[this.GameState.ActivePlayer]].Hand[this.HandIndex];
                var otherCard = drawCards.GameState.Inventories[drawCards.GameState.InventoryMap[drawCards.GameState.ActivePlayer]].Hand[drawCards.HandIndex];

                return card.CompareTo(otherCard);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerIndex = state.InventoryMap[this.PlayerToken];
            var playerInventory = state.Inventories[playerIndex];

            var discarded = (ActionCard)playerInventory.Hand[this.HandIndex];
            playerInventory = playerInventory.With(
                hand: playerInventory.Hand.RemoveAt(this.HandIndex));

            state = state.With(
                discardPile: state.DiscardPile.Add(discarded),
                inventories: state.Inventories.SetItem(
                    playerIndex,
                    playerInventory));

            return base.Apply(state);
        }
    }
}
