// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Cards
{
    using System.Collections.Generic;
    using GameTheory.Games.SevenDragons.Moves;

    /// <summary>
    /// Represents a Trade Hands card.
    /// </summary>
    public sealed class TradeHandsCard : ActionCard
    {
        /// <summary>
        /// The singleton instance of the <see cref="TradeHandsCard"/> class.
        /// </summary>
        public static readonly TradeHandsCard Instance = new TradeHandsCard();

        private TradeHandsCard()
            : base(Color.Black)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new[] { Resources.TradeHands };

        /// <inheritdoc />
        internal override IEnumerable<Move> GenerateActionMoves(GameState state)
        {
            foreach (var player in state.Players)
            {
                if (player != state.ActivePlayer)
                {
                    yield return new TradeHandsMove(state, player);
                }
            }
        }
    }
}
