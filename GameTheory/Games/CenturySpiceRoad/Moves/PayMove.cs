// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to pay to afford the next merchant card.
    /// </summary>
    public class PayMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="spice">The spice to pay.</param>
        public PayMove(GameState state, Spice spice)
            : base(state)
        {
            this.Spice = spice;
        }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the spice to pay.
        /// </summary>
        public Spice Spice { get; }

        /// <inheritdoc />
        public override string ToString() => $"Pay {this.Spice}";

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            if (state.MerchantCardIndexAfforded < state.MerchantCardTrack.Count - 1)
            {
                foreach (var spice in state.Inventory[state.ActivePlayer].Caravan.Keys)
                {
                    yield return new PayMove(state, spice);
                }
            }
        }

        /// <inheritdoc />
        internal override GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var pInventory = state.Inventory[activePlayer];
            pInventory = pInventory.With(
                caravan: pInventory.Caravan.Remove(this.Spice));

            var index = state.MerchantCardIndexAfforded;
            var merchantStall = state.MerchantCardTrack[index];
            merchantStall = merchantStall.With(
                spices: merchantStall.Spices.Add(this.Spice));

            state = state.With(
                inventory: state.Inventory.SetItem(activePlayer, pInventory),
                merchantCardIndexAfforded: index + 1,
                merchantCardTrack: state.MerchantCardTrack.SetItem(index, merchantStall),
                phase: Phase.Acquire);

            return base.Apply(state);
        }
    }
}
