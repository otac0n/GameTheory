﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a Point card in Century Spice Road.
    /// </summary>
    public sealed class PointCard : IComparable<PointCard>, ITokenFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointCard"/> class.
        /// </summary>
        /// <param name="points">The points awarded by this card.</param>
        /// <param name="cost">The cost of this card.</param>
        public PointCard(int points, EnumCollection<Spice> cost)
        {
            this.Points = points;
            this.Cost = cost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointCard"/> class.
        /// </summary>
        /// <param name="points">The points awarded by this card.</param>
        /// <param name="turmeric">The turmeric component of the cost.</param>
        /// <param name="saffron">The saffron component of the cost.</param>
        /// <param name="cardamom">The cardamom component of the cost.</param>
        /// <param name="cinnamon">The cinnamon component of the cost.</param>
        public PointCard(int points, int turmeric = 0, int saffron = 0, int cardamom = 0, int cinnamon = 0)
            : this(points, EnumCollection<Spice>.Empty
                  .Add(Spice.Turmeric, turmeric)
                  .Add(Spice.Saffron, saffron)
                  .Add(Spice.Cardamom, cardamom)
                  .Add(Spice.Cinnamon, cinnamon))
        {
        }

        /// <summary>
        /// Gets the cost of this card.
        /// </summary>
        public EnumCollection<Spice> Cost { get; }

        /// <inheritdoc/>
        public IList<object> FormatTokens => new object[] { "+", this.Points };

        /// <summary>
        /// Gets the points awarded by this card.
        /// </summary>
        public int Points { get; }

        /// <inheritdoc/>
        public int CompareTo(PointCard other)
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

            if ((comp = this.Points.CompareTo(other.Points)) != 0 ||
                (comp = this.Cost.CompareTo(other.Cost)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
