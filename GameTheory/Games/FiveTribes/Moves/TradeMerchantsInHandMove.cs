// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

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
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        public TradeMerchantsInHandMove(GameState state0)
            : base(state0, state0.ActivePlayer)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Take {0} resources", this.State.InHand.Count);
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];
            var resources = inventory.Resources.AddRange(state0.VisibleResources.Take(state0.InHand.Count));
            return state0.With(
                bag: state0.Bag.AddRange(state0.InHand),
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state0.Inventory.SetItem(player, inventory.With(resources: resources)),
                phase: Phase.TileAction,
                visibleResources: state0.VisibleResources.RemoveRange(0, state0.InHand.Count));
        }
    }
}
