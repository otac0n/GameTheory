// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    /// <summary>
    /// Represents a move for the payment of a specific amount of <see cref="Meeple">Meeples</see>.
    /// </summary>
    public class PayMeeplesMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Meeple> meeples;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayMeeplesMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeple">The cost, in <see cref="Meeple">Meeples</see>.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayMeeplesMove(GameState state0, Meeple meeple, Func<GameState, GameState> after)
            : this(state0, new EnumCollection<Meeple>(meeple), after)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayMeeplesMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeples">The cost, in <see cref="Meeple">Meeples</see>.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayMeeplesMove(GameState state0, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.meeples = meeples;
        }

        /// <summary>
        /// Gets the cost, in <see cref="Meeple">Meeples</see>.
        /// </summary>
        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        /// <inheritdoc />
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
