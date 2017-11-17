// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Describes the state of a single flower in the playing area.
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
            if (other == this)
            {
                return 0;
            }
            else if (other == null)
            {
                return 1;
            }

            int comp;

            if (this.Guardians != other.Guardians)
            {
                if ((comp = this.Guardians.Count.CompareTo(other.Guardians.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.Guardians.Count; i++)
                {
                    if ((comp = this.Guardians[i].CompareTo(other.Guardians[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if (this.Petals != other.Petals)
            {
                if ((comp = this.Petals.Count.CompareTo(other.Petals.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.Petals.Count; i++)
                {
                    if ((comp = this.Petals[i].CompareTo(other.Petals[i])) != 0)
                    {
                        return comp;
                    }
                }
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
