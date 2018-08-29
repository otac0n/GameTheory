// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a player's inventory.
    /// </summary>
    public sealed class Inventory : IComparable<Inventory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        /// <param name="goal">The player's goal.</param>
        /// <param name="hand">The cards in the player's hand.</param>
        public Inventory(Color goal, ImmutableList<Card> hand)
        {
            this.Hand = hand;
            this.Goal = goal;
        }

        /// <summary>
        /// Gets the cards in the player's hand.
        /// </summary>
        public ImmutableList<Card> Hand { get; }

        /// <summary>
        /// Gets the player's goal.
        /// </summary>
        public Color Goal { get; }

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
                (comp = EnumComparer<Color>.Default.Compare(this.Goal, other.Goal)) != 0)
            {
                return comp;
            }

            return 0;
        }

        internal Inventory With(
            Color? goal = null,
            ImmutableList<Card> hand = null)
        {
            return new Inventory(
                goal ?? this.Goal,
                hand ?? this.Hand);
        }
    }
}
