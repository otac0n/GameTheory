﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to choose spices to discard.
    /// </summary>
    public sealed class DiscardSpicesMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscardSpicesMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="spices">The spices to discard.</param>
        public DiscardSpicesMove(GameState state, EnumCollection<Spice> spices)
            : base(state)
        {
            this.Spices = spices;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.DiscardSpices, this.Spices);

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the available spices.
        /// </summary>
        public EnumCollection<Spice> Spices { get; }

        internal static IEnumerable<DiscardSpicesMove> GenerateMoves(GameState state)
        {
            var toDiscard = state.Inventory[state.ActivePlayer].Caravan.Count - GameState.CaravanLimit;

            foreach (var discardSpices in state.Inventory[state.ActivePlayer].Caravan.Combinations(toDiscard))
            {
                yield return new DiscardSpicesMove(state, discardSpices);
            }
        }

        internal static bool ShouldTransitionToPhase(GameState state)
        {
            return state.Inventory[state.ActivePlayer].Caravan.Count > GameState.CaravanLimit;
        }

        internal override GameState Apply(GameState state)
        {
            var pInventory = state.Inventory[state.ActivePlayer];
            var pSpices = pInventory.Caravan;

            pSpices = pSpices.RemoveRange(this.Spices);
            pInventory = pInventory.With(caravan: pSpices);

            state = state.With(
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory),
                phase: Phase.Play);

            return base.Apply(state);
        }
    }
}
