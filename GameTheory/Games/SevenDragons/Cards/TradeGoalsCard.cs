// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Cards
{
    using System.Collections.Generic;
    using GameTheory.Games.SevenDragons.Moves;

    /// <summary>
    /// Represents a Trade Goals card.
    /// </summary>
    public sealed class TradeGoalsCard : ActionCard
    {
        /// <summary>
        /// The singleton instance of the <see cref="TradeGoalsCard"/> class.
        /// </summary>
        public static readonly TradeGoalsCard Instance = new TradeGoalsCard();

        private TradeGoalsCard()
            : base(Color.Gold)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new[] { Resources.TradeGoals };

        /// <inheritdoc />
        internal override IEnumerable<Move> GenerateActionMoves(GameState state)
        {
            var playerIndex = state.InventoryMap[state.ActivePlayer];
            for (var i = 0; i < state.Inventories.Count; i++)
            {
                if (i != playerIndex)
                {
                    yield return new TradeGoalsMove(state, i);
                }
            }
        }
    }
}
