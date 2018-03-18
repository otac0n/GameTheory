// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;

    /// <summary>
    /// Provides utility methods working with enumerations.
    /// </summary>
    public static class EnumUtilities
    {
        /// <summary>
        /// Provides a strongly typed version of <see cref="Enum.GetValues(Type)"/>.
        /// </summary>
        /// <typeparam name="T">The enumeration type.</typeparam>
        /// <returns>An eunmerable collection of the values in the enumeration.</returns>
        public static T[] GetValues<T>() => (T[])Enum.GetValues(typeof(T));
    }
}
