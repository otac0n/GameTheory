// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    /// <summary>
    /// Represents a move for the payment of a specific amount of Gold Coins (GC).
    /// </summary>
    public class PayGoldMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly int gold;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayGoldMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="gold">The amount of Gold Coins (GC) that will be paid.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayGoldMove(GameState state0, int gold, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.gold = gold;
        }

        /// <summary>
        /// Gets the amount of Gold Coins (GC) that will be paid.
        /// </summary>
        public int Gold
        {
            get { return this.gold; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Pay {0} Gold", this.gold);
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return this.after(state0.With(
                inventory: state0.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins - this.gold))));
        }
    }
}
