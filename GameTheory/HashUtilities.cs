// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides utility methods for composing comparable objects.
    /// </summary>
    public static class HashUtilities
    {
        /// <summary>
        /// Gets a seed value for creating hash codes.
        /// </summary>
        public const int Seed = 17;

        /// <summary>
        /// Combines a running current hash code with an other hash code.
        /// </summary>
        /// <param name="hash">The current hash code.</param>
        /// <param name="other">The other hash code to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Combine(ref int hash, int other)
        {
            hash = unchecked((hash * 33) + other);
        }
    }
}
