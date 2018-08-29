// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.SevenDragons.Cards;

    /// <summary>
    /// Represents a move to place a dragon card on the table.
    /// </summary>
    public sealed class PlaceCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="handIndex">The index of the card being played.</param>
        /// <param name="point">The point to which the card will be played.</param>
        /// <param name="orientation">The orientation of the card being played.</param>
        public PlaceCardMove(GameState state, int handIndex, Point point, DragonCard orientation)
            : base(state)
        {
            this.HandIndex = handIndex;
            this.Point = point;
            this.Orientation = orientation;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.PlayDragonCard, this.Orientation, this.Point);

        /// <summary>
        /// Gets the index of the card being played.
        /// </summary>
        public int HandIndex { get; }

        /// <summary>
        /// Gets the point to which the card will be played.
        /// </summary>
        public Point Point { get; }

        /// <summary>
        /// Gets the orientation of the card being played.
        /// </summary>
        public DragonCard Orientation { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PlaceCardMove placeCard)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.HandIndex.CompareTo(placeCard.HandIndex)) != 0 ||
                    (comp = this.Point.CompareTo(placeCard.Point)) != 0 ||
                    (comp = this.Orientation.CompareTo(placeCard.Orientation)) != 0)
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

        internal static IEnumerable<PlaceCardMove> GenerateMoves(GameState state)
        {
            if (state.Phase == Phase.Play)
            {
                var playerIndex = state.InventoryMap[state.ActivePlayer];
                var playerInventory = state.Inventories[playerIndex];
                var hand = playerInventory.Hand;

                var adjacent = state.GetEmptyAdjacent();

                for (var i = 0; i < hand.Count; i++)
                {
                    if (hand[i] is DragonCard card)
                    {
                        var orientations = card == card.Reversed
                            ? new[] { card }
                            : new[] { card, card.Reversed };
                        foreach (var orientation in orientations)
                        {
                            foreach (var point in adjacent)
                            {
                                if (Enumerable.Range(0, DragonCard.Grid.Count).Any(j =>
                                {
                                    var color = orientation.Colors[j];
                                    return color == Color.Rainbow || state.GetMatchingAdjacent(point, DragonCard.Grid[j], color, color).Any();
                                }))
                                {
                                    yield return new PlaceCardMove(state, i, point, orientation);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerIndex = state.InventoryMap[this.PlayerToken];
            var playerInventory = state.Inventories[playerIndex];

            playerInventory = playerInventory.With(
                hand: playerInventory.Hand.RemoveAt(this.HandIndex));

            var bonusCards = 0;
            var matching = Enumerable.Range(0, DragonCard.Grid.Count).Where(i =>
            {
                var color = this.Orientation.Colors[i];
                return color != Color.Rainbow && state.GetMatchingAdjacent(this.Point, DragonCard.Grid[i], color, Color.Rainbow).Any();
            });
            bonusCards = matching.Select(i => this.Orientation.Colors[i]).Distinct().Count() - 1;

            state = state.With(
                inventories: state.Inventories.SetItem(
                    playerIndex,
                    playerInventory),
                table: state.Table.SetItem(this.Point, this.Orientation));

            bonusCards = Math.Min(bonusCards, state.Deck.Count);
            if (bonusCards > 0)
            {
                return state.WithInterstitialState(new DrawingBonusCards(bonusCards));
            }
            else
            {
                return base.Apply(state);
            }
        }

        private class DrawingBonusCards : InterstitialState
        {
            public DrawingBonusCards(int cardCount)
            {
                this.CardCount = cardCount;
            }

            public int CardCount { get; }

            public override int CompareTo(InterstitialState other)
            {
                if (other is DrawingBonusCards drawingBonusCards)
                {
                    return this.CardCount.CompareTo(drawingBonusCards.CardCount);
                }
                else
                {
                    return base.CompareTo(other);
                }
            }

            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                yield return new DrawCardsMove(state, this.CardCount);
            }
        }
    }
}
