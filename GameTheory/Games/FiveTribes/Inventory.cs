// -----------------------------------------------------------------------
// <copyright file="Inventory.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a player's inventory.
    /// </summary>
    public class Inventory
    {
        private readonly ImmutableList<Djinn> djinns;
        private readonly int goldCoins;
        private readonly EnumCollection<Meeple> meeples;
        private readonly EnumCollection<Resource> resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        public Inventory()
        {
            this.djinns = ImmutableList<Djinn>.Empty;
            this.meeples = EnumCollection<Meeple>.Empty;
            this.goldCoins = 50;
            this.resources = EnumCollection<Resource>.Empty;
        }

        private Inventory(ImmutableList<Djinn> djinns, EnumCollection<Meeple> meeples, int goldCoins, EnumCollection<Resource> resources)
        {
            this.djinns = djinns;
            this.meeples = meeples;
            this.goldCoins = goldCoins;
            this.resources = resources;
        }

        /// <summary>
        /// Gets the player's <see cref="Djinn">Djinns</see>.
        /// </summary>
        public ImmutableList<Djinn> Djinns
        {
            get { return this.djinns; }
        }

        /// <summary>
        /// Gets the player's Gold Coins (GC).
        /// </summary>
        public int GoldCoins
        {
            get { return this.goldCoins; }
        }

        /// <summary>
        /// Gets the player's <see cref="Meeple">Meeples</see>.
        /// </summary>
        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        /// <summary>
        /// Gets the player's <see cref="Resource">Resources</see>.
        /// </summary>
        public EnumCollection<Resource> Resources
        {
            get { return this.resources; }
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
                djinns ?? this.djinns,
                meeples ?? this.meeples,
                goldCoins ?? this.goldCoins,
                resources ?? this.resources);
        }
    }
}
