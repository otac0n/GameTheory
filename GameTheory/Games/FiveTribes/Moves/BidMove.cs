namespace GameTheory.Games.FiveTribes.Moves
{
    public class BidMove : Move
    {
        private readonly int cost;
        private readonly int index;

        public BidMove(GameState state0, int index, int cost)
            : base(state0, state0.ActivePlayer, s2 =>
            {
                var player = s2.ActivePlayer;
                var inventory = s2.Inventory[player];
                var newQueue = s2.BidOrderTrack.Dequeue();
                return s2.With(
                    bidOrderTrack: newQueue,
                    turnOrderTrack: s2.TurnOrderTrack.SetItem(index, player),
                    inventory: s2.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins - cost)),
                    phase: newQueue.IsEmpty ? Phase.MoveTurnMarker : Phase.Bid);
            })
        {
            this.index = index;
            this.cost = cost;
        }

        public int Cost
        {
            get { return this.cost; }
        }

        public int Index
        {
            get { return this.index; }
        }

        public override string ToString()
        {
            return string.Format("Bid {0}", this.cost);
        }
    }
}
