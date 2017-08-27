// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;
    using GameTheory.Games.CenturySpiceRoad.MerchantCards;

    /// <summary>
    /// Represents a move to play a spice card.
    /// </summary>
    public sealed class SpiceCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the card to play.</param>
        public SpiceCardMove(GameState state, int index)
            : base(state)
        {
            this.Index = index;
        }

        /// <summary>
        /// Gets the index of the card to play.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc/>
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Play ", this.State.Inventory[this.PlayerToken].Hand[this.Index] };

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var hand = state.Inventory[state.ActivePlayer].Hand;
            for (var i = 0; i < hand.Count; i++)
            {
                if (hand[i] is SpiceCard)
                {
                    yield return new SpiceCardMove(state, i);
                }
            }
        }

        /// <inheritdoc/>
        internal override GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var pInventory = state.Inventory[activePlayer];
            var card = (SpiceCard)pInventory.Hand[this.Index];

            pInventory = pInventory.With(
                caravan: pInventory.Caravan.AddRange(card.Spices),
                hand: pInventory.Hand.RemoveAt(this.Index),
                playedCards: pInventory.PlayedCards.Add(card));

            state = state.With(
                inventory: state.Inventory.SetItem(activePlayer, pInventory));

            return base.Apply(state);
        }
    }
}
