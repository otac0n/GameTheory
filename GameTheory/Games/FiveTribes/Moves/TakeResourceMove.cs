namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class TakeResourceMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly int index;

        public TakeResourceMove(GameState state0, int index)
            : this(state0, index, s => s)
        {
        }

        public TakeResourceMove(GameState state0, int index, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var player = s1.ActivePlayer;
                var inventory = s1.Inventory[player];

                return after(s1.With(
                    inventory: s1.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.Add(s1.VisibleResources[index]))),
                    visibleResources: s1.VisibleResources.RemoveAt(index)));
            })
        {
            this.index = index;
            this.after = after;
        }

        public int Index
        {
            get { return this.index; }
        }

        public Resource Resource
        {
            get { return this.State.VisibleResources[this.index]; }
        }

        public override string ToString()
        {
            return string.Format("Take {0}", this.State.VisibleResources[this.index]);
        }
    }
}
