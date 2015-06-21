namespace GameTheory.Games.FiveTribes.Moves
{
    public class TakeMeeplesInHandMove : Move
    {
        public TakeMeeplesInHandMove(GameState state0)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var player = s1.ActivePlayer;
                var inventory = s1.Inventory[player];
                return s1.With(
                    inHand: EnumCollection<Meeple>.Empty,
                    inventory: s1.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.AddRange(s1.InHand))),
                    phase: Phase.TileAction);
            })
        {
        }

        public override string ToString()
        {
            return string.Format("Take {0}", string.Join(",", this.State.InHand));
        }
    }
}
