// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    /// <summary>
    /// Represents a move to assassinate specific <see cref="Meeple">Meeples</see> from a specific player's inventory.
    /// </summary>
    public class AssassinatePlayerMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Meeple> meeples;
        private readonly PlayerToken victim;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinatePlayerMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="victim">The <see cref="PlayerToken"/> whose <see cref="Meeple">Meeples</see> will be assassinated.</param>
        /// <param name="meeples">The <see cref="Meeple">Meeples</see> that will be assassinated.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public AssassinatePlayerMove(GameState state0, PlayerToken victim, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.meeples = meeples;
            this.victim = victim;
        }

        /// <summary>
        /// Gets the <see cref="Meeple">Meeples</see> that will be assassinated.
        /// </summary>
        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> whose <see cref="Meeple">Meeples</see> will be assassinated.
        /// </summary>
        public PlayerToken Victim
        {
            get { return this.victim; }
        }

        /// <inheritdoc />
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
