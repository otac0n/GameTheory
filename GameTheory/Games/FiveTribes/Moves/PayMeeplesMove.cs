// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move for the payment of a specific amount of <see cref="Meeple">Meeples</see>.
    /// </summary>
    public sealed class PayMeeplesMove : Move
    {
        private readonly Func<GameState, GameState> after;

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
            : base(state)
        {
            this.after = after;
            this.Meeples = meeples;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Pay ", this.Meeples };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the cost, in <see cref="Meeple">Meeples</see>.
        /// </summary>
        public EnumCollection<Meeple> Meeples { get; }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return this.after(state.With(
                bag: state.Bag.AddRange(this.Meeples),
                inventory: state.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.RemoveRange(this.Meeples)))));
        }
    }
}
