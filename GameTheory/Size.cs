// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a 2D size as a tuple of 32-bit integers.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "The primary use case of the type doesn't warrant the collection suffix.")]
    public struct Size : IComparable<Size>, IEquatable<Size>, IReadOnlyList<Point>
    {
        private int height;
        private int width;

        /// <summary>
        /// Initializes a new instance of the <see cref="Size"/> struct.
        /// </summary>
        /// <param name="width">The x-axis (horizontal) dimension of the size.</param>
        /// <param name="height">The y-axis (vertival) dimension of the size.</param>
        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        /// <inheritdoc />
        public int Count => this.width * this.height;

        /// <summary>
        /// Gets the y-axis (vertival) dimension of the size.
        /// </summary>
        public int Height => this.height;

        /// <summary>
        /// Gets the x-axis (horizontal) dimension of the size.
        /// </summary>
        public int Width => this.width;

        /// <inheritdoc />
        public Point this[int index]
        {
            get
            {
                if (index < 0 || index >= this.width * this.height)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return new Point(index % this.width, index / this.width);
            }
        }

        /// <summary>
        /// Compares two <see cref="Size"/> objects. The result specifies whether they are unequal.
        /// </summary>
        /// <param name="left">The first <see cref="Size"/> to compare.</param>
        /// <param name="right">The second <see cref="Size"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Size left, Size right) => !(left == right);

        /// <summary>
        /// Compares two <see cref="Size"/> objects. The result specifies whether they are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Size"/> to compare.</param>
        /// <param name="right">The second <see cref="Size"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Size left, Size right) => left.Equals(right);

        /// <inheritdoc />
        public int CompareTo(Size other)
        {
            int comp;
            if ((comp = this.width.CompareTo(other.width)) != 0)
            {
                return comp;
            }

            return this.height.CompareTo(other.height);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Size other && this.Equals(other);

        /// <inheritdoc />
        public bool Equals(Size other) =>
            this.width == other.width &&
            this.height == other.height;

        /// <inheritdoc />
        public IEnumerator<Point> GetEnumerator()
        {
            for (var y = 0; y < this.height; y++)
            {
                for (var x = 0; x < this.width; x++)
                {
                    yield return new Point(x, y);
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.width);
            HashUtilities.Combine(ref hash, this.height);
            return hash;
        }

        /// <summary>
        /// Determines the index of the specific <see cref="Point"/> in the <see cref="Size"/>, assuming the same ordering of points as the enumerator.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to locate in the <see cref="Size"/>.</param>
        /// <returns>The index of the <see cref="Point"/> if found in the <see cref="Size"/>; otherwise, -1.</returns>
        public int IndexOf(Point point) => this.IndexOf(point.X, point.Y);

        /// <summary>
        /// Determines the index of the specific <see cref="Point"/> in the <see cref="Size"/>, assuming the same ordering of points as the enumerator.
        /// </summary>
        /// <param name="x">The x-coordinate of the <see cref="Point"/> to locate in the <see cref="Size"/>.</param>
        /// <param name="y">The y-coordinate of the <see cref="Point"/> to locate in the <see cref="Size"/>.</param>
        /// <returns>The index of the <see cref="Point"/> if found in the <see cref="Size"/>; otherwise, -1.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public int IndexOf(int x, int y)
        {
            if (x < 0 || x >= this.width)
            {
                return -1;
            }

            if (y < 0 || y >= this.height)
            {
                return -1;
            }

            return this.width * y + x;
        }

        /// <inheritdoc />
        public override string ToString() => $"({this.width}*{this.height})";
    }
}
