namespace GameTheory.Games.FiveTribes.Moves
{
    public class SellMerchandiceMove : Move
    {
        private readonly EnumCollection<Resource> resources;

        public SellMerchandiceMove(GameState state0, EnumCollection<Resource> resources)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var player = s1.ActivePlayer;
                var inventory = s1.Inventory[player];

                return s1.With(
                    inventory: s1.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.RemoveRange(resources), goldCoins: inventory.GoldCoins + GameState.SuitValues[resources.Count])),
                    resourceDiscards: s1.ResourceDiscards.AddRange(resources));
            })
        {
            this.resources = resources;
        }

        public override string ToString()
        {
            return string.Format("Trade {0} for {1}", string.Join(",", this.resources), GameState.SuitValues[this.resources.Count]);
        }
    }
}
