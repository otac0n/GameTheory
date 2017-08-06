// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
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
        public Inventory()
            : this(ImmutableList<DevelopmentCard>.Empty, EnumCollection<Token>.Empty, ImmutableList<DevelopmentCard>.Empty, ImmutableList<Noble>.Empty)
        {
        }

        private Inventory(
            ImmutableList<DevelopmentCard> hand,
            EnumCollection<Token> tokens,
            ImmutableList<DevelopmentCard> developmentCards,
            ImmutableList<Noble> nobles)
        {
            this.Hand = hand;
            this.Tokens = tokens;
            this.DevelopmentCards = developmentCards;
            this.Nobles = nobles;
        }

        /// <summary>
        /// Gets the development cards owned by the player.
        /// </summary>
        public ImmutableList<DevelopmentCard> DevelopmentCards { get; }

        /// <summary>
        /// Gets the cards in the player's hand.
        /// </summary>
        public ImmutableList<DevelopmentCard> Hand { get; }

        /// <summary>
        /// Gets the Nobles that have visited the player.
        /// </summary>
        public ImmutableList<Noble> Nobles { get; }

        /// <summary>
        /// Gets the tokens owned by the player.
        /// </summary>
        public EnumCollection<Token> Tokens { get; }

        internal Inventory With(
            ImmutableList<DevelopmentCard> hand = null,
            EnumCollection<Token> tokens = null,
            ImmutableList<DevelopmentCard> developmentCards = null,
            ImmutableList<Noble> nobles = null)
        {
            return new Inventory(
                hand ?? this.Hand,
                tokens ?? this.Tokens,
                developmentCards ?? this.DevelopmentCards,
                nobles ?? this.Nobles);
        }

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

            if ((comp = this.Tokens.CompareTo(other.Tokens)) != 0)
            {
                return comp;
            }

            if (this.DevelopmentCards != other.DevelopmentCards)
            {
                if ((comp = this.DevelopmentCards.Count.CompareTo(other.DevelopmentCards.Count)) != 0)
                {
                    return comp;
                }

                for (int i = 0; i < this.DevelopmentCards.Count; i++)
                {
                    if ((comp = this.DevelopmentCards[i].CompareTo(other.DevelopmentCards[i])) != 0)
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

                for (int i = 0; i < this.Hand.Count; i++)
                {
                    if ((comp = this.Hand[i].CompareTo(other.Hand[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if (this.Nobles != other.Nobles)
            {
                if ((comp = this.Nobles.Count.CompareTo(other.Nobles.Count)) != 0)
                {
                    return comp;
                }

                for (int i = 0; i < this.Nobles.Count; i++)
                {
                    if ((comp = this.Nobles[i].CompareTo(other.Nobles[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            return 0;
        }
    }
}
