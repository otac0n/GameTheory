namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PayGoldMove : Move
    {
        private readonly int gold;

        public PayGoldMove(GameState state0, int gold, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var player = s1.ActivePlayer;
                var inventory = s1.Inventory[player];

                return after(s1.With(
                    inventory: s1.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins - gold))));
            })
        {
            this.gold = gold;
        }

        public int Gold
        {
            get { return this.gold; }
        }

        public override string ToString()
        {
            return string.Format("Pay {0} Gold", this.gold);
        }
    }
}
