// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a player's inventory.
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        /// <param name="spices">The initial spices in the inventory.</param>
        /// <param name="hand">The initial hand in the inventory.</param>
        public Inventory(EnumCollection<Spice> spices, ImmutableList<MerchantCard> hand)
            : this(spices, hand, ImmutableList<MerchantCard>.Empty, ImmutableList<PointCard>.Empty, EnumCollection<Token>.Empty)
        {
        }

        private Inventory(
            EnumCollection<Spice> caravan,
            ImmutableList<MerchantCard> hand,
            ImmutableList<MerchantCard> playedCards,
            ImmutableList<PointCard> pointCards,
            EnumCollection<Token> tokens)
        {
            this.Caravan = caravan;
            this.Hand = hand;
            this.PlayedCards = playedCards;
            this.PointCards = pointCards;
            this.Tokens = tokens;
        }

        /// <summary>
        /// Gets the spices owned by the player.
        /// </summary>
        public EnumCollection<Spice> Caravan { get; }

        /// <summary>
        /// Gets the cards in the player's hand.
        /// </summary>
        public ImmutableList<MerchantCard> Hand { get; }

        public ImmutableList<MerchantCard> PlayedCards { get; }

        public ImmutableList<PointCard> PointCards { get; }

        public EnumCollection<Token> Tokens { get; }

        internal Inventory With(
            EnumCollection<Spice> caravan = null,
            ImmutableList<MerchantCard> hand = null,
            ImmutableList<MerchantCard> playedCards = null,
            ImmutableList<PointCard> pointCards = null,
            EnumCollection<Token> tokens = null)
        {
            return new Inventory(
                caravan ?? this.Caravan,
                hand ?? this.Hand,
                playedCards ?? this.PlayedCards,
                pointCards ?? this.PointCards,
                tokens ?? this.Tokens);
        }
    }
}
