// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to upgrade spices.
    /// </summary>
    public sealed class UpgradeMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="startingSpice">The starting spice.</param>
        public UpgradeMove(GameState state, Spice startingSpice)
            : base(state)
        {
            this.StartingSpice = startingSpice;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Upgrade ", this.StartingSpice, " to ", this.UpgradedSpice };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the starting spice.
        /// </summary>
        public Spice StartingSpice { get; }

        /// <summary>
        /// Gets the upgraded spice.
        /// </summary>
        public Spice UpgradedSpice => (Spice)((int)this.StartingSpice + 1);

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            foreach (var spice in state.Inventory[state.ActivePlayer].Caravan.Keys)
            {
                if (spice != Spice.Cinnamon)
                {
                    yield return new UpgradeMove(state, spice);
                }
            }
        }

        /// <inheritdoc />
        internal override GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var pInventory = state.Inventory[activePlayer];

            pInventory = pInventory.With(
                caravan: pInventory.Caravan.Remove(this.StartingSpice).Add(this.UpgradedSpice));

            var upgradesRemaining = state.UpgradesRemaining - 1;

            state = state.With(
                inventory: state.Inventory.SetItem(activePlayer, pInventory),
                upgradesRemaining: upgradesRemaining,
                phase: upgradesRemaining > 0 ? state.Phase : Phase.Play);

            return base.Apply(state);
        }
    }
}
