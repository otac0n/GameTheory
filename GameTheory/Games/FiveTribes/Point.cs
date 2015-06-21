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

    public struct Point
    {
        private readonly byte index;

        public Point(int index)
        {
            Contract.Requires(index >= 0 && index < Sultanate.Width * Sultanate.Height);

            this.index = (byte)index;
        }

        public Point(int x, int y)
        {
            Contract.Requires(x >= 0 && x < Sultanate.Width);
            Contract.Requires(y >= 0 && y < Sultanate.Height);

            this.index = (byte)(Sultanate.Width * y + x);
        }

        public int X
        {
            get { return this.index % Sultanate.Width; }
        }

        public int Y
        {
            get { return this.index / Sultanate.Width; }
        }

        public static implicit operator int(Point a)
        {
            return a.index;
        }

        public static implicit operator Point(int a)
        {
            return new Point(a);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        public override string ToString()
        {
            return "(" + this.X + ", " + this.Y + ")";
        }
    }
}
