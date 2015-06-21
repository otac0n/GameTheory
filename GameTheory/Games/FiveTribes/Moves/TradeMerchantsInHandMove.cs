namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Linq;

    public class TradeMerchantsInHandMove : Move
    {
        public TradeMerchantsInHandMove(GameState state0)
            : base(state0, state0.ActivePlayer, s =>
            {
                var player = s.ActivePlayer;
                var inventory = s.Inventory[player];
                var resources = inventory.Resources.AddRange(s.VisibleResources.Take(s.InHand.Count));
                return s.With(
                    bag: s.Bag.AddRange(s.InHand),
                    inHand: EnumCollection<Meeple>.Empty,
                    inventory: s.Inventory.SetItem(player, inventory.With(resources: resources)),
                    phase: Phase.TileAction,
                    visibleResources: s.VisibleResources.RemoveRange(0, s.InHand.Count));
            })
        {
        }

        public override string ToString()
        {
            return string.Format("Take {0} resources", this.State.InHand.Count);
        }
    }
}
