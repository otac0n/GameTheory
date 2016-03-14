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
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeple">The cost, in <see cref="Meeple">Meeples</see>.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayMeeplesMove(GameState state, Meeple meeple, Func<GameState, GameState> after)
            : this(state, new EnumCollection<Meeple>(meeple), after)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayMeeplesMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeples">The cost, in <see cref="Meeple">Meeples</see>.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayMeeplesMove(GameState state, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state, state.ActivePlayer)
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
            return $"Pay {string.Join(",", this.meeples)}";
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return this.after(state.With(
                bag: state.Bag.AddRange(this.meeples),
                inventory: state.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.RemoveRange(this.meeples)))));
        }
    }
}
