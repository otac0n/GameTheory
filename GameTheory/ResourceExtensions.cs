// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System.Resources;

    /// <summary>
    /// Provides extensions for working with the string resources in this assembly.
    /// </summary>
    public static class ResourceExtensions
    {
        /// <summary>
        /// Fetches the string value for the enum from the specified resource manager.
        /// </summary>
        /// <typeparam name="T">The type of the enum resource string to be retrieved.</typeparam>
        /// <param name="resources">The collection of resources that contains the enum value.</param>
        /// <param name="value">The value to retrieve.</param>
        /// <returns>The string vale for the specified enum.</returns>
        public static string GetEnumString<T>(this ResourceManager resources, T value)
        {
            var enumString = value.ToString();
            return resources.GetString($"{typeof(T).Name}_{enumString}") ?? enumString;
        }
    }
}
