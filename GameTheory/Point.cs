// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a 2D point as a tuple of 32-bit integers.
    /// </summary>
    public struct Point : IComparable<Point>, IEquatable<Point>
    {
        private int x;
        private int y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the <see cref="Point"/>.</param>
        /// <param name="y">The y coordinate of the <see cref="Point"/>.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Gets the x coordinate of the <see cref="Point"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "X is meaningful in the context of coordinates.")]
        public int X => this.x;

        /// <summary>
        /// Gets the y coordinate of the <see cref="Point"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y", Justification = "Y is meaningful in the context of coordinates.")]
        public int Y => this.y;

        /// <summary>
        /// Compares two <see cref="Point"/> objects. The result specifies whether they are unequal.
        /// </summary>
        /// <param name="left">The first <see cref="Point"/> to compare.</param>
        /// <param name="right">The second <see cref="Point"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Point left, Point right) => !(left == right);

        /// <summary>
        /// Compares two <see cref="Point"/> objects. The result specifies whether they are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Point"/> to compare.</param>
        /// <param name="right">The second <see cref="Point"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Point left, Point right) => left.Equals(right);

        /// <inheritdoc />
        public int CompareTo(Point other)
        {
            int comp;
            if ((comp = this.x.CompareTo(other.x)) != 0)
            {
                return comp;
            }

            return this.y.CompareTo(other.y);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Point other && this.Equals(other);

        /// <inheritdoc />
        public bool Equals(Point other) =>
            this.x == other.x &&
            this.y == other.y;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.x);
            HashUtilities.Combine(ref hash, this.y);
            return hash;
        }

        /// <inheritdoc />
        public override string ToString() => $"({this.x}, {this.y})";
    }
}
