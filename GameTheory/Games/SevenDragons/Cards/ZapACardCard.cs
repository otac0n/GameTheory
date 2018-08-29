// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Cards
{
    using System.Collections.Generic;
    using GameTheory.Games.SevenDragons.Moves;

    /// <summary>
    /// Represents a Zap a Card card.
    /// </summary>
    public sealed class ZapACardCard : ActionCard
    {
        /// <summary>
        /// The singleton instance of the <see cref="ZapACardCard"/> class.
        /// </summary>
        public static readonly ZapACardCard Instance = new ZapACardCard();

        private ZapACardCard()
            : base(Color.Red)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new[] { Resources.ZapACard };

        /// <inheritdoc />
        internal override IEnumerable<Move> GenerateActionMoves(GameState state)
        {
            foreach (var key in state.Table.Keys)
            {
                if (key.X != 0 || key.Y != 0)
                {
                    yield return new ZapCardMove(state, key);
                }
            }
        }
    }
}
