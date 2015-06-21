namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PayMeeplesMove : Move
    {
        private readonly EnumCollection<Meeple> meeples;

        public PayMeeplesMove(GameState state0, Meeple meeple, Func<GameState, GameState> after)
            : this(state0, new EnumCollection<Meeple>(meeple), after)
        {
        }

        public PayMeeplesMove(GameState state0, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var player = s1.ActivePlayer;
                var inventory = s1.Inventory[player];

                return after(s1.With(
                    bag: s1.Bag.AddRange(meeples),
                    inventory: s1.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.RemoveRange(meeples)))));
            })
        {
            this.meeples = meeples;
        }

        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        public override string ToString()
        {
            return string.Format("Pay {0}", string.Join(",", this.meeples));
        }
    }
}
