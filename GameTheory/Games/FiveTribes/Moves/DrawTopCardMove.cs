// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a move to draw the top card from the <see cref="Resource"/> pile.
    /// </summary>
    internal class DrawTopCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawTopCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public DrawTopCardMove(GameState state)
            : base(state, state.ActivePlayer)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Draw the top card from the Resource Pile";
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            ImmutableList<Resource> dealt;
            var newDiscards = state.ResourceDiscards;
            var newResourcesPile = state.ResourcePile.Deal(1, out dealt, ref newDiscards);
            var newInventory = inventory.With(resources: inventory.Resources.AddRange(dealt));

            return state.With(
                inventory: state.Inventory.SetItem(player, newInventory),
                resourceDiscards: newDiscards,
                resourcePile: newResourcesPile);
        }
    }
}
