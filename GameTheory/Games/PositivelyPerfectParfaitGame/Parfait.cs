// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame
{
    using System;

    /// <summary>
    /// Represents a parfait.
    /// </summary>
    public class Parfait : IComparable<Parfait>
    {
        /// <summary>
        /// An empty parfait.
        /// </summary>
        public static readonly Parfait Empty = new Parfait(EnumCollection<Flavor>.Empty, false);

        private Parfait(
            EnumCollection<Flavor> flavors,
            bool cherry)
        {
            this.Flavors = flavors;
            this.Cherry = cherry;
        }

        /// <summary>
        /// Gets a value indicating whether or not the parfait has a cherry.
        /// </summary>
        public bool Cherry { get; }

        /// <summary>
        /// Gets the flavors of ice cream scoops in the parfait.
        /// </summary>
        public EnumCollection<Flavor> Flavors { get; }

        /// <inheritdoc/>
        public int CompareTo(Parfait other)
        {
            if (other == null)
            {
                return 1;
            }

            var comp = 0;

            if ((comp = this.Cherry.CompareTo(other.Cherry)) != 0 ||
                (comp = this.Flavors.CompareTo(other.Flavors)) != 0)
            {
                return comp;
            }

            return comp;
        }

        internal Parfait With(
            EnumCollection<Flavor> flavors = null,
            bool? cherry = null)
        {
            return new Parfait(
                flavors ?? this.Flavors,
                cherry ?? this.Cherry);
        }
    }
}
