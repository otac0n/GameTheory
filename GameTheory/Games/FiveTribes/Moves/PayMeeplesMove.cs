﻿// -----------------------------------------------------------------------
// <copyright file="PayMeeplesMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PayMeeplesMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Meeple> meeples;

        public PayMeeplesMove(GameState state0, Meeple meeple, Func<GameState, GameState> after)
            : this(state0, new EnumCollection<Meeple>(meeple), after)
        {
        }

        public PayMeeplesMove(GameState state0, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
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

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return this.after(state0.With(
                bag: state0.Bag.AddRange(this.meeples),
                inventory: state0.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.RemoveRange(this.meeples)))));
        }
    }
}