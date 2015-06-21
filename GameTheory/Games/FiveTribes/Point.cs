// -----------------------------------------------------------------------
// <copyright file="Point.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System.Diagnostics.Contracts;

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
            Contract.Requires(index >= 0 && index < Sultanate.Width * Sultanate.Height);

            this.index = (byte)index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the <see cref="Point"/>.</param>
        /// <param name="y">The y coordinate of the <see cref="Point"/>.</param>
        public Point(int x, int y)
        {
            Contract.Requires(x >= 0 && x < Sultanate.Width);
            Contract.Requires(y >= 0 && y < Sultanate.Height);

            this.index = (byte)(Sultanate.Width * y + x);
        }

        /// <summary>
        /// Gets the x coordinate of the <see cref="Point"/>.
        /// </summary>
        public int X
        {
            get { return this.index % Sultanate.Width; }
        }

        /// <summary>
        /// Gets the y coordinate of the <see cref="Point"/>.
        /// </summary>
        public int Y
        {
            get { return this.index / Sultanate.Width; }
        }

        /// <summary>
        /// Converts the specified <see cref="Point"/> object to an index.
        /// </summary>
        /// <param name="a">The <see cref="Point"/> to be converted.</param>
        /// <returns>The index of the <see cref="Point"/>.</returns>
        public static implicit operator int(Point a)
        {
            return a.index;
        }

        /// <summary>
        /// Converts the specified index to a <see cref="Point"/> object.
        /// </summary>
        /// <param name="a">The index to be converted.</param>
        /// <returns>The <see cref="Point"/> object that results from the conversion.</returns>
        public static implicit operator Point(int a)
        {
            return new Point(a);
        }

        /// <summary>
        /// Compares two <see cref="Point"/> objects. The result specifies whether they are unequal.
        /// </summary>
        /// <param name="a">The first <see cref="Point"/> to compare.</param>
        /// <param name="b">The second <see cref="Point"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="a"/> and <paramref name="b"/> differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Compares two <see cref="Point"/> objects. The result specifies whether they are equal.
        /// </summary>
        /// <param name="a">The first <see cref="Point"/> to compare.</param>
        /// <param name="b">The second <see cref="Point"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="a"/> and <paramref name="b"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "(" + this.X + ", " + this.Y + ")";
        }
    }
}
