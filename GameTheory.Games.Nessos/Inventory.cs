// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


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
            this.OwnedCards = ImmutableList<Card>.Empty;
            this.Hand = ImmutableList<Card>.Empty;
        }

        private Inventory(
            ImmutableList<Card> ownedCards,
            ImmutableList<Card> hand)
        {
            this.OwnedCards = ownedCards;
            this.Hand = hand;
        }

         /// <summary>
        /// Gets the player's hand.
        /// </summary>
        public ImmutableList<Card> Hand { get; }

        /// <summary>
        /// Gets the player's stack of played cards.
        /// </summary>
        public ImmutableList<Card> OwnedCards { get; }

        public int Score { get; }

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

            if ((comp = CompareUtilities.CompareLists(this.Hand, other.Hand)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.OwnedCards, other.OwnedCards)) != 0)
            {
                return comp;
            }

            return 0;
        }

        internal Inventory With(
            ImmutableList<Card> ownedCards = null,
            ImmutableList<Card> hand = null)
        {
            return new Inventory(
                ownedCards: ownedCards ?? this.OwnedCards,
                hand: hand ?? this.Hand);
        }
    }
}
