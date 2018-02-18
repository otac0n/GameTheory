// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a player's inventory.
    /// </summary>
    public class Inventory : IComparable<Inventory>
    {
        /// <summary>
        /// The number of guardians each player starts with.
        /// </summary>
        public const int StartingGuardians = 2;

        /// <summary>
        /// The number of cards each player starts with;
        /// </summary>
        public const int StartingHandCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        /// <param name="deck">The player's deck of cards.</param>
        public Inventory(ImmutableList<PetalCard> deck)
            : this(StartingGuardians, ImmutableList<PetalCard>.Empty, deck, ImmutableList<PetalCard>.Empty, 0, SpecialPower.None)
        {
            this.Deck = this.Deck.Deal(StartingHandCount, out var dealt);
            this.Hand = dealt;
        }

        private Inventory(
            int guardians,
            ImmutableList<PetalCard> hand,
            ImmutableList<PetalCard> deck,
            ImmutableList<PetalCard> scoringPile,
            int scoringTokens,
            SpecialPower specialPowers)
        {
            this.Guardians = guardians;
            this.Hand = hand;
            this.Deck = deck;
            this.ScoringPile = scoringPile;
            this.ScoringTokens = scoringTokens;
            this.SpecialPowers = specialPowers;
        }

        /// <summary>
        /// Gets the cards in the player's deck.
        /// </summary>
        public ImmutableList<PetalCard> Deck { get; }

        /// <summary>
        /// Gets the number of guardians held by the player.
        /// </summary>
        public int Guardians { get; }

        /// <summary>
        /// Gets the cards in the player's hand.
        /// </summary>
        public ImmutableList<PetalCard> Hand { get; }

        /// <summary>
        /// Gets the list of cards in the player's scoring pile.
        /// </summary>
        public ImmutableList<PetalCard> ScoringPile { get; }

        /// <summary>
        /// Gets the number of scoring tokens that the player has collected.
        /// </summary>
        public int ScoringTokens { get; }

        /// <summary>
        /// Gets the special powers the player has activated.
        /// </summary>
        public SpecialPower SpecialPowers { get; }

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

            if ((comp = this.Guardians.CompareTo(other.Guardians)) != 0 ||
                (comp = this.ScoringTokens.CompareTo(other.ScoringTokens)) != 0 ||
                (comp = EnumComparer<SpecialPower>.Default.Compare(this.SpecialPowers, other.SpecialPowers)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Hand, other.Hand)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Deck, other.Deck)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.ScoringPile, other.ScoringPile)) != 0)
            {
                return comp;
            }

            return 0;
        }

        internal Inventory With(
            int? guardians = null,
            ImmutableList<PetalCard> hand = null,
            ImmutableList<PetalCard> deck = null,
            ImmutableList<PetalCard> scoringPile = null,
            int? scoringTokens = null,
            SpecialPower? specialPowers = null)
        {
            return new Inventory(
                guardians ?? this.Guardians,
                hand ?? this.Hand,
                deck ?? this.Deck,
                scoringPile ?? this.ScoringPile,
                scoringTokens ?? this.ScoringTokens,
                specialPowers ?? this.SpecialPowers);
        }
    }
}
