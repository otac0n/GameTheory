// -----------------------------------------------------------------------
// <copyright file="PayGoldMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PayGoldMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly int gold;

        public PayGoldMove(GameState state0, int gold, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.gold = gold;
        }

        public int Gold
        {
            get { return this.gold; }
        }

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
