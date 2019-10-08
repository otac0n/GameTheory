// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a player's inventory.
    /// </summary>
    public sealed class Inventory : IComparable<Inventory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        /// <param name="hand">The player's hand.</param>
        public Inventory(ImmutableArray<Card> hand)
            : this(hand, EnumCollection<Card>.Empty, 0, false)

        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        /// <param name="hand">The player's hand.</param>
        /// <param name="discards">The player's discards.</param>
        /// <param name="tokens">The player's tokens of affection.</param>
        /// <param name="handRevealed">A value indicating whether or not the player's hand has been revealed.</param>
        public Inventory(ImmutableArray<Card> hand, EnumCollection<Card> discards, int tokens, bool handRevealed)
        {
            this.Hand = hand;
            this.Discards = discards;
            this.Tokens = tokens;
            this.HandRevealed = handRevealed;
        }

        /// <summary>
        /// Gets the player's discards.
        /// </summary>
        public EnumCollection<Card> Discards { get; }

        /// <summary>
        /// Gets the player's hand.
        /// </summary>
        public ImmutableArray<Card> Hand { get; }

        /// <summary>
        /// Gets a value indicating whether or not the player's hand has been revealed.
        /// </summary>
        public bool HandRevealed { get; }

        /// <summary>
        /// Gets the player's tokens of affection.
        /// </summary>
        public int Tokens { get; }

        /// <inheritdoc/>
        public int CompareTo(Inventory other)
        {
            int comp;

            if ((comp = this.Tokens.CompareTo(other.Tokens)) != 0 ||
                (comp = this.HandRevealed.CompareTo(other.HandRevealed)) != 0 ||
                (comp = CompareUtilities.CompareEnumLists(this.Hand, other.Hand)) != 0 ||
                (comp = this.Discards.CompareTo(other.Discards)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;

            HashUtilities.Combine(ref hash, this.Tokens);
            HashUtilities.Combine(ref hash, this.HandRevealed ? 0 : 1);

            if (this.Hand.Length > 0)
            {
                HashUtilities.Combine(ref hash, (int)this.Hand[0]);
            }

            return hash;
        }
    }
}
