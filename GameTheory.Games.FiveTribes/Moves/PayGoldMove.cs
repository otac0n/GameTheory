﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move for the payment of a specific amount of Gold Coins (GC).
    /// </summary>
    public sealed class PayGoldMove : Move
    {
        private readonly Func<GameState, GameState> after;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayGoldMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="gold">The amount of Gold Coins (GC) that will be paid.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayGoldMove(GameState state, int gold, Func<GameState, GameState> after)
            : base(state)
        {
            this.after = after;
            this.Gold = gold;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.PayGold, this.Gold);

        /// <summary>
        /// Gets the amount of Gold Coins (GC) that will be paid.
        /// </summary>
        public int Gold { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return this.after(state.With(
                inventory: state.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins - this.Gold))));
        }
    }
}
