// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos
{
    using System;


    /// <summary>
    /// Represents a player's inventory.
    /// </summary>
    public sealed class Inventory : IComparable<Inventory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        public Inventory()
        {
            this.OwnedCards = EnumCollection<Card>.Empty;
            this.Hand = EnumCollection<Card>.Empty;
        }

        private Inventory(
            EnumCollection<Card> ownedCards,
            EnumCollection<Card> hand)
        {
            this.OwnedCards = ownedCards;
            this.Hand = hand;
        }

         /// <summary>
        /// Gets the player's hand.
        /// </summary>
        public EnumCollection<Card> Hand { get; }

        /// <summary>
        /// Gets the player's stack of played cards.
        /// </summary>
        public EnumCollection<Card> OwnedCards { get; }

        /// <inheritdoc/>
        public int CompareTo(Inventory other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int comp;

            if ((comp = this.Hand.CompareTo(other.Hand)) != 0 ||
                (comp = this.OwnedCards.CompareTo(other.OwnedCards)) != 0)
            {
                return comp;
            }

            return 0;
        }

        internal Inventory With(
            EnumCollection<Card> ownedCards = null,
            EnumCollection<Card> hand = null)
        {
            return new Inventory(
                ownedCards: ownedCards ?? this.OwnedCards,
                hand: hand ?? this.Hand);
        }
    }
}
