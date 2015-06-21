namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class AssassinatePlayerMove : Move
    {
        private readonly EnumCollection<Meeple> meeples;
        private readonly PlayerToken victim;

        public AssassinatePlayerMove(GameState state0, PlayerToken victim, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var inventory = s1.Inventory[victim];
                var newState = s1.With(
                    bag: s1.Bag.AddRange(s1.InHand).AddRange(meeples),
                    inHand: EnumCollection<Meeple>.Empty,
                    inventory: s1.Inventory.SetItem(victim, inventory.With(meeples: inventory.Meeples.RemoveRange(meeples))));

                foreach (var owner in newState.Players)
                {
                    foreach (var djinn in newState.Inventory[owner].Djinns)
                    {
                        newState = djinn.HandleAssassination(owner, newState, victim, meeples);
                    }
                }

                return after(newState);
            })
        {
            this.victim = victim;
            this.meeples = meeples;
        }

        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        public PlayerToken Victim
        {
            get { return this.victim; }
        }

        public override string ToString()
        {
            return string.Format("Assassinate {0}'s {1}", this.victim, string.Join(",", this.meeples));
        }
    }
}
