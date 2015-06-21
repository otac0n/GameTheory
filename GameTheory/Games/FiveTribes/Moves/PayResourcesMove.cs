// -----------------------------------------------------------------------
// <copyright file="PayResourcesMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PayResourcesMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Resource> resources;

        public PayResourcesMove(GameState state0, Resource resource, Func<GameState, GameState> after)
            : this(state0, new EnumCollection<Resource>(resource), after)
        {
        }

        public PayResourcesMove(GameState state0, EnumCollection<Resource> resources, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.resources = resources;
        }

        public EnumCollection<Resource> Resources
        {
            get { return this.resources; }
        }

        public override string ToString()
        {
            return string.Format("Pay {0}", string.Join(",", this.resources));
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return this.after(state0.With(
                inventory: state0.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.RemoveRange(this.resources))),
                resourceDiscards: state0.ResourceDiscards.AddRange(this.resources)));
        }
    }
}
