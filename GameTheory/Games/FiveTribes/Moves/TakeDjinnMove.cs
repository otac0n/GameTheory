// -----------------------------------------------------------------------
// <copyright file="TakeDjinnMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

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
            : base(state0, state0.ActivePlayer)
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

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return this.after(state0.With(
                inventory: state0.Inventory.SetItem(player, inventory.With(djinns: inventory.Djinns.Add(state0.VisibleDjinns[this.index]))),
                visibleDjinns: state0.VisibleDjinns.RemoveAt(this.index)));
        }
    }
}
