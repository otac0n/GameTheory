// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a player's inventory.
    /// </summary>
    public sealed class Inventory : IComparable<Inventory>
    {
        /// <summary>
        /// Get the bid representing a pass.
        /// </summary>
        public static readonly int PassingBid = -1;

        private static readonly EnumCollection<Card> DefaultHand = EnumCollection<Card>.Empty
            .Add(Card.Flower, 3)
            .Add(Card.Skull);

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        public Inventory()
        {
            this.Stack = ImmutableList<Card>.Empty;
            this.Hand = DefaultHand;
            this.Bid = 0;
            this.Revealed = EnumCollection<Card>.Empty;
            this.ChallengesWon = 0;
            this.PresentedCards = ImmutableList<Card>.Empty;
            this.Discards = EnumCollection<Card>.Empty;
        }

        private Inventory(
            ImmutableList<Card> stack,
            EnumCollection<Card> hand,
            int bid,
            EnumCollection<Card> revealed,
            int challengesWon,
            ImmutableList<Card> presentedCards,
            EnumCollection<Card> discards)
        {
            this.Stack = stack;
            this.Hand = hand;
            this.Bid = bid;
            this.Revealed = revealed;
            this.ChallengesWon = challengesWon;
            this.PresentedCards = presentedCards;
            this.Discards = discards;
        }

        /// <summary>
        /// Gets the player's current bid.
        /// </summary>
        /// <remarks>
        /// A bid of <c>0</c> represents that the player has not yet bid this round.
        /// A bid less than <c>0</c> represents that the player has passed.
        /// </remarks>
        public int Bid { get; }

        /// <summary>
        /// Gets the number of challenges the player has won.
        /// </summary>
        public int ChallengesWon { get; }

        /// <summary>
        /// Gets the discarded cards.
        /// </summary>
        public EnumCollection<Card> Discards { get; }

        /// <summary>
        /// Gets the player's hand.
        /// </summary>
        public EnumCollection<Card> Hand { get; }

        /// <summary>
        /// Gets the cards the player is presenting for removal.
        /// </summary>
        public ImmutableList<Card> PresentedCards { get; }

        /// <summary>
        /// Gets the cards that have been revealed from the player's stack.
        /// </summary>
        public EnumCollection<Card> Revealed { get; }

        /// <summary>
        /// Gets the player's stack of played cards.
        /// </summary>
        public ImmutableList<Card> Stack { get; }

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

            if ((comp = this.ChallengesWon.CompareTo(other.ChallengesWon)) != 0 ||
                (comp = this.Bid.CompareTo(other.Bid)) != 0 ||
                (comp = this.Hand.CompareTo(other.Hand)) != 0 ||
                (comp = this.Revealed.CompareTo(other.Revealed)) != 0 ||
                (comp = CompareUtilities.CompareEnumLists(this.PresentedCards, other.PresentedCards)) != 0 ||
                (comp = CompareUtilities.CompareEnumLists(this.Stack, other.Stack)) != 0)
            {
                return comp;
            }

            return 0;
        }

        internal Inventory With(
            ImmutableList<Card> stack = null,
            EnumCollection<Card> hand = null,
            int? bid = null,
            EnumCollection<Card> revealed = null,
            int? challengesWon = null,
            ImmutableList<Card> presentedCards = null,
            EnumCollection<Card> discards = null)
        {
            return new Inventory(
                stack: stack ?? this.Stack,
                hand: hand ?? this.Hand,
                bid: bid ?? this.Bid,
                revealed: revealed ?? this.Revealed,
                challengesWon: challengesWon ?? this.ChallengesWon,
                presentedCards: presentedCards ?? this.PresentedCards,
                discards: discards ?? this.Discards);
        }
    }
}
