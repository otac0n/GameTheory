// -----------------------------------------------------------------------
// <copyright file="AssassinatePlayerMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class AssassinatePlayerMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Meeple> meeples;
        private readonly PlayerToken victim;

        public AssassinatePlayerMove(GameState state0, PlayerToken victim, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.meeples = meeples;
            this.victim = victim;
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

        internal override GameState Apply(GameState state0)
        {
            var inventory = state0.Inventory[this.victim];
            var newState = state0.With(
                bag: state0.Bag.AddRange(state0.InHand).AddRange(this.meeples),
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state0.Inventory.SetItem(this.victim, inventory.With(meeples: inventory.Meeples.RemoveRange(this.meeples))));

            foreach (var owner in newState.Players)
            {
                foreach (var djinn in newState.Inventory[owner].Djinns)
                {
                    newState = djinn.HandleAssassination(owner, newState, this.victim, this.meeples);
                }
            }

            return this.after(newState);
        }
    }
}
