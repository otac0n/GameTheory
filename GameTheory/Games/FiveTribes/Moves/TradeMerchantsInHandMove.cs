﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Linq;

    /// <summary>
    /// Represents a move to trade the <see cref="Meeple.Merchant">Merchants</see> in hand for <see cref="Resource">Resources</see>.
    /// </summary>
    public class TradeMerchantsInHandMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeMerchantsInHandMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public TradeMerchantsInHandMove(GameState state)
            : base(state, state.ActivePlayer)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Take {this.State.InHand.Count} resources";
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];
            var resources = inventory.Resources.AddRange(state.VisibleResources.Take(state.InHand.Count));
            return state.With(
                bag: state.Bag.AddRange(state.InHand),
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state.Inventory.SetItem(player, inventory.With(resources: resources)),
                phase: Phase.TileAction,
                visibleResources: state.VisibleResources.RemoveRange(0, state.InHand.Count));
        }
    }
}
