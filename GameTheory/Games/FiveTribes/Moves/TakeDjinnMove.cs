namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class TakeDjinnMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly int index;

        public TakeDjinnMove(GameState state0, int index)
            : this(state0, index, s => s)
        {
        }

        public TakeDjinnMove(GameState state0, int index, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var player = s1.ActivePlayer;
                var inventory = s1.Inventory[player];

                return after(s1.With(
                    inventory: s1.Inventory.SetItem(player, inventory.With(djinns: inventory.Djinns.Add(s1.VisibleDjinns[index]))),
                    visibleDjinns: s1.VisibleDjinns.RemoveAt(index)));
            })
        {
            this.index = index;
            this.after = after;
        }

        public Djinn Djinn
        {
            get { return this.State.VisibleDjinns[this.index]; }
        }

        public int Index
        {
            get { return this.index; }
        }

        public override string ToString()
        {
            return string.Format("Take {0}", this.Djinn);
        }
    }
}
