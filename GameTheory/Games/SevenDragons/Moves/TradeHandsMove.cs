// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to trade hands with another player.
    /// </summary>
    public sealed class TradeHandsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeHandsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="otherPlayer">The other player with which to trade hands.</param>
        public TradeHandsMove(GameState state, PlayerToken otherPlayer)
            : base(state)
        {
            this.OtherPlayer = otherPlayer;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.TradeHandsWithPlayer, this.OtherPlayer);

        /// <summary>
        /// Gets the other player with which to trade hands.
        /// </summary>
        public PlayerToken OtherPlayer { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is TradeHandsMove tradeHands)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.OtherPlayer.CompareTo(tradeHands.OtherPlayer)) != 0)
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

        /// <inheritdoc />
        internal override GameState Apply(GameState state)
        {
            var playerIndex = state.InventoryMap[state.ActivePlayer];
            var playerInventory = state.Inventories[playerIndex];

            var otherIndex = state.InventoryMap[this.OtherPlayer];
            var otherInventory = state.Inventories[otherIndex];

            var originalHand = playerInventory.Hand;
            playerInventory = playerInventory.With(hand: otherInventory.Hand);
            otherInventory = otherInventory.With(hand: originalHand);

            state = state.With(
                inventories: state.Inventories.SetItem(playerIndex, playerInventory).SetItem(otherIndex, otherInventory));

            return base.Apply(state);
        }
    }
}
