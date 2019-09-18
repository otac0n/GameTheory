// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
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
        {
            this.Djinns = ImmutableList<Djinn>.Empty;
            this.Meeples = EnumCollection<Meeple>.Empty;
            this.GoldCoins = 50;
            this.Resources = EnumCollection<Resource>.Empty;
        }

        private Inventory(ImmutableList<Djinn> djinns, EnumCollection<Meeple> meeples, int goldCoins, EnumCollection<Resource> resources)
        {
            this.Djinns = djinns;
            this.Meeples = meeples;
            this.GoldCoins = goldCoins;
            this.Resources = resources;
        }

        /// <summary>
        /// Gets the player's <see cref="Djinn">Djinns</see>.
        /// </summary>
        public ImmutableList<Djinn> Djinns { get; }

        /// <summary>
        /// Gets the player's Gold Coins (GC).
        /// </summary>
        public int GoldCoins { get; }

        /// <summary>
        /// Gets the player's <see cref="Meeple">Meeples</see>.
        /// </summary>
        public EnumCollection<Meeple> Meeples { get; }

        /// <summary>
        /// Gets the player's <see cref="Resource">Resources</see>.
        /// </summary>
        public EnumCollection<Resource> Resources { get; }

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

            if ((comp = this.GoldCoins.CompareTo(other.GoldCoins)) != 0 ||
                (comp = this.Meeples.CompareTo(other.Meeples)) != 0 ||
                (comp = this.Resources.CompareTo(other.Resources)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Djinns, other.Djinns)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <summary>
        /// Creates a new <see cref="Inventory"/>, and updates the specified values.
        /// </summary>
        /// <param name="djinns"><c>null</c> to keep the existing value, or any other value to update <see cref="Djinns"/>.</param>
        /// <param name="goldCoins"><c>null</c> to keep the existing value, or any other value to update <see cref="GoldCoins"/>.</param>
        /// <param name="meeples"><c>null</c> to keep the existing value, or any other value to update <see cref="Meeples"/>.</param>
        /// <param name="resources"><c>null</c> to keep the existing value, or any other value to update <see cref="Resources"/>.</param>
        /// <returns>The new <see cref="Inventory"/>.</returns>
        public Inventory With(ImmutableList<Djinn> djinns = null, int? goldCoins = null, EnumCollection<Meeple> meeples = null, EnumCollection<Resource> resources = null)
        {
            return new Inventory(
                djinns ?? this.Djinns,
                meeples ?? this.Meeples,
                goldCoins ?? this.GoldCoins,
                resources ?? this.Resources);
        }
    }
}
