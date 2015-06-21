// -----------------------------------------------------------------------
// <copyright file="SellMerchandiceMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    public class SellMerchandiceMove : Move
    {
        private readonly EnumCollection<Resource> resources;

        public SellMerchandiceMove(GameState state0, EnumCollection<Resource> resources)
            : base(state0, state0.ActivePlayer)
        {
            this.resources = resources;
        }

        public override string ToString()
        {
            return string.Format("Trade {0} for {1}", string.Join(",", this.resources), GameState.SuitValues[this.resources.Count]);
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return state0.With(
                inventory: state0.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.RemoveRange(this.resources), goldCoins: inventory.GoldCoins + GameState.SuitValues[this.resources.Count])),
                resourceDiscards: state0.ResourceDiscards.AddRange(this.resources));
        }
    }
}
