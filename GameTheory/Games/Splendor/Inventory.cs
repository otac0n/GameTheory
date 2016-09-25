// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
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
        public ImmutableList<Noble> Nobles { get; private set; }

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
    }
}
