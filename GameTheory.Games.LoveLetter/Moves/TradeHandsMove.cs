// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to choose another player to compare hands with.
    /// </summary>
    public sealed class TradeHandsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeHandsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="targetPlayer">The target player.</param>
        public TradeHandsMove(GameState state, PlayerToken targetPlayer)
            : base(state)
        {
            this.TargetPlayer = targetPlayer;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.TradeHands, this.TargetPlayer);

        /// <summary>
        /// Gets the target player.
        /// </summary>
        public PlayerToken TargetPlayer { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is TradeHandsMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.TargetPlayer.CompareTo(move.TargetPlayer)) != 0)
                {
                    return comp;
                }

                return 0;
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory;

            var activePlayer = this.PlayerToken;
            var activePlayerInventory = inventory[activePlayer];
            var activeHand = activePlayerInventory.Hand;

            var targetPlayer = this.TargetPlayer;
            var targetPlayerInventory = inventory[targetPlayer];
            var targetHand = targetPlayerInventory.Hand;

            activePlayerInventory = activePlayerInventory.With(
                hand: targetHand);
            targetPlayerInventory = targetPlayerInventory.With(
                hand: activeHand);

            inventory = inventory
                .SetItem(activePlayer, activePlayerInventory)
                .SetItem(targetPlayer, targetPlayerInventory);

            state = state.With(
                inventory: inventory);

            return base.Apply(state);
        }
    }
}
