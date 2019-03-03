// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Provides utility methods working with enumerations.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    public static class EnumUtilities<T>
    {
        private static readonly IReadOnlyList<T> Values = Enum.GetValues(typeof(T)).Cast<T>().ToImmutableList();

        /// <summary>
        /// Provides a strongly typed version of <see cref="Enum.GetValues(Type)"/>.
        /// </summary>
        /// <returns>An eunmerable collection of the values in the enumeration.</returns>
        public static IReadOnlyList<T> GetValues() => Values;
    }
}
