// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Cards
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.SevenDragons.Moves;

    /// <summary>
    /// The base class for action cards.
    /// </summary>
    public abstract class ActionCard : Card, ITokenFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionCard"/> class.
        /// </summary>
        /// <param name="color">The color of the action card.</param>
        protected ActionCard(Color color)
        {
            this.Color = color;
        }

        /// <summary>
        /// Gets the color of the action card.
        /// </summary>
        public Color Color { get; }

        /// <inheritdoc />
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc />
        public override int CompareTo(Card other)
        {
            if (other is ActionCard actionCard)
            {
                int comp;

                if ((comp = EnumComparer<Color>.Default.Compare(this.Color, actionCard.Color)) != 0)
                {
                    return comp;
                }
            }

            return base.CompareTo(other);
        }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var playerIndex = state.InventoryMap[state.ActivePlayer];
            var playerInventory = state.Inventories[playerIndex];
            var hand = playerInventory.Hand;

            var adjacent = state.GetEmptyAdjacent();

            for (var i = 0; i < hand.Count; i++)
            {
                if (hand[i] is ActionCard card)
                {
                    if (card.GenerateActionMoves(state).Any())
                    {
                        yield return new PlayActionMove(state, i);
                    }

                    yield return new DiscardCardMove(state, i);
                }
            }
        }

        internal abstract IEnumerable<Move> GenerateActionMoves(GameState state);
    }
}
