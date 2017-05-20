// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a point in the <see cref="Sultanate"/>.
    /// </summary>
    public struct Point
    {
        private readonly byte index;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="index">The index of the <see cref="Point"/> within the <see cref="Sultanate"/>.</param>
        public Point(int index)
        {
            if (index < 0 || index >= Sultanate.Width * Sultanate.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            this.index = (byte)index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the <see cref="Point"/>.</param>
        /// <param name="y">The y coordinate of the <see cref="Point"/>.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public Point(int x, int y)
        {
            if (x < 0 || x >= Sultanate.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (y < 0 || y >= Sultanate.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            this.index = (byte)(Sultanate.Width * y + x);
        }

        /// <summary>
        /// Gets the x coordinate of the <see cref="Point"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "X is meaningful in the context of coordinates.")]
        public int X
        {
            get { return this.index % Sultanate.Width; }
        }

        /// <summary>
        /// Gets the y coordinate of the <see cref="Point"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y", Justification = "Y is meaningful in the context of coordinates.")]
        public int Y
        {
            get { return this.index / Sultanate.Width; }
        }

        /// <summary>
        /// Converts the specified <see cref="Point"/> object to an index.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to be converted.</param>
        /// <returns>The index of the <see cref="Point"/>.</returns>
        public static implicit operator int(Point point)
        {
            return point.index;
        }

        /// <summary>
        /// Converts the specified index to a <see cref="Point"/> object.
        /// </summary>
        /// <param name="index">The index to be converted.</param>
        /// <returns>The <see cref="Point"/> object that results from the conversion.</returns>
        public static implicit operator Point(int index)
        {
            return new Point(index);
        }

        /// <summary>
        /// Compares two <see cref="Point"/> objects. The result specifies whether they are unequal.
        /// </summary>
        /// <param name="left">The first <see cref="Point"/> to compare.</param>
        /// <param name="right">The second <see cref="Point"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Compares two <see cref="Point"/> objects. The result specifies whether they are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Point"/> to compare.</param>
        /// <param name="right">The second <see cref="Point"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Point && ((Point)obj).index == this.index;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.index;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "(" + this.X + ", " + this.Y + ")";
        }
    }
}
