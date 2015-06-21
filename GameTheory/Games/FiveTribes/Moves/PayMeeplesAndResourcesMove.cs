namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Linq;

    public class PayMeeplesAndResourcesMove : Move
    {
        private readonly EnumCollection<Meeple> meeples;
        private readonly EnumCollection<Resource> resources;

        public PayMeeplesAndResourcesMove(GameState state0, Meeple meeple, Resource resource, Func<GameState, GameState> after)
            : this(state0, new EnumCollection<Meeple>(meeple), new EnumCollection<Resource>(resource), after)
        {
        }

        public PayMeeplesAndResourcesMove(GameState state0, EnumCollection<Meeple> meeples, EnumCollection<Resource> resources, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var player = s1.ActivePlayer;
                var inventory = s1.Inventory[player];

                return after(s1.With(
                    bag: s1.Bag.AddRange(meeples),
                    inventory: s1.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.RemoveRange(meeples), resources: inventory.Resources.RemoveRange(resources))),
                    resourceDiscards: s1.ResourceDiscards.AddRange(resources)));
            })
        {
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
    }
}
