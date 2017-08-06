// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;

    public class UpgradeMove : Move
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
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the starting spice.
        /// </summary>
        public Spice StartingSpice { get; }

        /// <inheritdoc />
        public override string ToString() => $"Upgrade {this.StartingSpice} to {(Spice)((int)this.StartingSpice + 1)}";

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
                caravan: pInventory.Caravan.Remove(this.StartingSpice).Add((Spice)((int)this.StartingSpice + 1)));

            var upgradesRemaining = state.UpgradesRemaining - 1;

            state = state.With(
                inventory: state.Inventory.SetItem(activePlayer, pInventory),
                upgradesRemaining: upgradesRemaining,
                phase: upgradesRemaining > 0 ? state.Phase : Phase.Play);

            return base.Apply(state);
        }
    }
}
