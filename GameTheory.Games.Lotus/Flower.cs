// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents the state of a single flower in the playing area.
    /// </summary>
    public class Flower : IComparable<Flower>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Flower"/> class.
        /// </summary>
        public Flower()
        {
            this.Guardians = ImmutableList<PlayerToken>.Empty;
            this.Petals = ImmutableList<PetalCard>.Empty;
        }

        private Flower(ImmutableList<PlayerToken> guardians, ImmutableList<PetalCard> petals)
        {
            this.Guardians = guardians;
            this.Petals = petals;
        }

        /// <summary>
        /// Gets the guardians on this flower.
        /// </summary>
        public ImmutableList<PlayerToken> Guardians { get; }

        /// <summary>
        /// Gets the petals currently played to this flower.
        /// </summary>
        public ImmutableList<PetalCard> Petals { get; }

        /// <inheritdoc/>
        public int CompareTo(Flower other)
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

            if ((comp = CompareUtilities.CompareLists(this.Guardians, other.Guardians)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Petals, other.Petals)) != 0)
            {
                return comp;
            }

            return 0;
        }

        internal Flower With(
            ImmutableList<PlayerToken> guardians = null,
            ImmutableList<PetalCard> petals = null)
        {
            return new Flower(
                guardians ?? this.Guardians,
                petals ?? this.Petals);
        }
    }
}
