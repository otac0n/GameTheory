// -----------------------------------------------------------------------
// <copyright file="PayMeeplesAndResourcesMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Linq;

    public class PayMeeplesAndResourcesMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Meeple> meeples;
        private readonly EnumCollection<Resource> resources;

        public PayMeeplesAndResourcesMove(GameState state0, Meeple meeple, Resource resource, Func<GameState, GameState> after)
            : this(state0, new EnumCollection<Meeple>(meeple), new EnumCollection<Resource>(resource), after)
        {
        }

        public PayMeeplesAndResourcesMove(GameState state0, EnumCollection<Meeple> meeples, EnumCollection<Resource> resources, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.meeples = meeples;
            this.resources = resources;
        }

        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        public EnumCollection<Resource> Resources
        {
            get { return this.resources; }
        }

        public override string ToString()
        {
            return string.Format("Pay {0}", string.Join(",", this.meeples.Cast<object>().Concat(this.resources.Cast<object>())));
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return this.after(state0.With(
                bag: state0.Bag.AddRange(this.meeples),
                inventory: state0.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.RemoveRange(this.meeples), resources: inventory.Resources.RemoveRange(this.resources))),
                resourceDiscards: state0.ResourceDiscards.AddRange(this.resources)));
        }
    }
}
