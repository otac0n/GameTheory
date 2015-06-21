// -----------------------------------------------------------------------
// <copyright file="TakeResourceMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

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
            : base(state0, state0.ActivePlayer)
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

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return this.after(state0.With(
                inventory: state0.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.Add(state0.VisibleResources[this.index]))),
                visibleResources: state0.VisibleResources.RemoveAt(this.index)));
        }
    }
}
