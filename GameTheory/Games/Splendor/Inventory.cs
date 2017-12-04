// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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

        /// <inheritdoc />
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

            if ((comp = this.Tokens.CompareTo(other.Tokens)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.DevelopmentCards, other.DevelopmentCards)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Hand, other.Hand)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Nobles, other.Nobles)) != 0)
            {
                return comp;
            }

            return 0;
        }

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
    }
}
