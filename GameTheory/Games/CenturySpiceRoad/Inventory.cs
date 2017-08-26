﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a player's inventory.
    /// </summary>
    public class Inventory : IComparable<Inventory>
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

        /// <summary>
        /// Gets the list of cards in the player's played pile.
        /// </summary>
        public ImmutableList<MerchantCard> PlayedCards { get; }

        /// <summary>
        /// Gets the list of point cards that the player has collected.
        /// </summary>
        public ImmutableList<PointCard> PointCards { get; }

        /// <summary>
        /// Gets the tokens that the player has collected.
        /// </summary>
        public EnumCollection<Token> Tokens { get; }

        /// <inheritdoc/>
        public int CompareTo(Inventory other)
        {
            if (other == this)
            {
                return 0;
            }
            else if (other == null)
            {
                return 1;
            }

            int comp;

            if ((comp = this.Caravan.CompareTo(other.Caravan)) != 0 ||
                (comp = this.Tokens.CompareTo(other.Tokens)) != 0)
            {
                return comp;
            }

            if (this.PointCards != other.PointCards)
            {
                if ((comp = this.PointCards.Count.CompareTo(other.PointCards.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.PointCards.Count; i++)
                {
                    if ((comp = this.PointCards[i].CompareTo(other.PointCards[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if (this.Hand != other.Hand)
            {
                if ((comp = this.Hand.Count.CompareTo(other.Hand.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.Hand.Count; i++)
                {
                    if ((comp = this.Hand[i].CompareTo(other.Hand[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if (this.PlayedCards != other.PlayedCards)
            {
                if ((comp = this.PlayedCards.Count.CompareTo(other.PlayedCards.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.PlayedCards.Count; i++)
                {
                    if ((comp = this.PlayedCards[i].CompareTo(other.PlayedCards[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            return 0;
        }

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
