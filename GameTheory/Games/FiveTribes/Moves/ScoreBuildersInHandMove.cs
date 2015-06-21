namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Linq;

    public class ScoreBuildersInHandMove : Move
    {
        public ScoreBuildersInHandMove(GameState state0, int boost)
            : base(state0, state0.ActivePlayer, s2 =>
            {
                var blueTiles = Sultanate.GetSquarePoints(s2.LastPoint).Count(p => s2.Sultanate[p].Tile.Color == TileColor.Blue);
                var player = s2.ActivePlayer;
                var inventory = s2.Inventory[player];
                var score = (s2.InHand.Count + boost) * blueTiles * s2.ScoreTables[player].BuilderMultiplier;

                return s2.With(
                    bag: s2.Bag.AddRange(s2.InHand),
                    inHand: EnumCollection<Meeple>.Empty,
                    inventory: s2.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins + score)),
                    phase: Phase.TileAction);
            })
        {
        }

        public override string ToString()
        {
            return string.Format("Score {0}", string.Join(",", this.State.InHand));
        }
    }
}
