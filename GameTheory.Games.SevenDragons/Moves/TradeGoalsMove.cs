// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to trade goals with another position.
    /// </summary>
    public sealed class TradeGoalsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeGoalsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="otherIndex">The index of the other position with wich to trade goals.</param>
        public TradeGoalsMove(GameState state, int otherIndex)
            : base(state)
        {
            this.OtherIndex = otherIndex;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => this.GameState.InventoryMap.Values.Any(v => v == this.OtherIndex)
            ? FormatUtilities.ParseStringFormat(Resources.TradeGoalsWithPosition, this.OtherIndex)
            : FormatUtilities.ParseStringFormat(Resources.TradeGoalsWithPositionAndPlayer, this.OtherIndex, this.GameState.InventoryMap.First(kvp => kvp.Value == this.OtherIndex).Key);

        /// <summary>
        /// Gets the index of the other position with wich to trade goals.
        /// </summary>
        public int OtherIndex { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is TradeGoalsMove tradeGoals)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.OtherIndex.CompareTo(tradeGoals.OtherIndex)) != 0)
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

            var otherInventory = state.Inventories[this.OtherIndex];

            var originalGoal = playerInventory.Goal;
            playerInventory = playerInventory.With(goal: otherInventory.Goal);
            otherInventory = otherInventory.With(goal: originalGoal);

            state = state.With(
                inventories: state.Inventories.SetItem(playerIndex, playerInventory).SetItem(this.OtherIndex, otherInventory));

            return base.Apply(state);
        }
    }
}
