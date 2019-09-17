// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to draw a card from the deck.
    /// </summary>
    public sealed class DrawCardsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCardsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="cardCount">The count of cards being drawn.</param>
        public DrawCardsMove(GameState state, int cardCount = 1)
            : base(state)
        {
            this.CardCount = cardCount;
        }

        /// <summary>
        /// Gets the count of cards being drawn.
        /// </summary>
        public int CardCount { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens => this.CardCount == 1
            ? new object[] { Resources.DrawCard }
            : FormatUtilities.ParseStringFormat(Resources.DrawCards, this.CardCount);

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is DrawCardsMove drawCards)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.CardCount.CompareTo(drawCards.CardCount)) != 0)
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

        internal static IEnumerable<DrawCardsMove> GenerateMoves(GameState state)
        {
            if (state.Phase == Phase.Draw && state.Deck.Count > 0)
            {
                yield return new DrawCardsMove(state);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerIndex = state.InventoryMap[this.PlayerToken];
            var playerInventory = state.Inventories[playerIndex];

            var dealt = state.Deck.GetRange(state.Deck.Count - this.CardCount, this.CardCount);
            var deck = state.Deck.RemoveRange(state.Deck.Count - this.CardCount, this.CardCount);
            playerInventory = playerInventory.With(
                hand: playerInventory.Hand.AddRange(dealt));

            state = state.With(
                deck: deck,
                inventories: state.Inventories.SetItem(
                    playerIndex,
                    playerInventory));

            return base.Apply(state);
        }
    }
}
