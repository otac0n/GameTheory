namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PayResourcesMove : Move
    {
        private readonly EnumCollection<Resource> resources;

        public PayResourcesMove(GameState state0, Resource resource, Func<GameState, GameState> after)
            : this(state0, new EnumCollection<Resource>(resource), after)
        {
        }

        public PayResourcesMove(GameState state0, EnumCollection<Resource> resources, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var player = s1.ActivePlayer;
                var inventory = s1.Inventory[player];

                return after(s1.With(
                    inventory: s1.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.RemoveRange(resources))),
                    resourceDiscards: s1.ResourceDiscards.AddRange(resources)));
            })
        {
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
    }
}
