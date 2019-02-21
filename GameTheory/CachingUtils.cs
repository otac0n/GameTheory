// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;

    /// <summary>
    /// Provides utilities for caching.
    /// </summary>
    public static class CachingUtils
    {
        /// <summary>
        /// Automatically updates a weak reference cache.
        /// </summary>
        /// <typeparam name="T">The type of value in the cache.</typeparam>
        /// <param name="cache">The cache to search and update.</param>
        /// <param name="getValue">A function that will provide values when the cache is empty.</param>
        /// <returns>The cached value.</returns>
        public static T WeakRefernceCache<T>(ref WeakReference<T> cache, Func<T> getValue)
            where T : class
        {
            var created = cache != null;
            if (created && cache.TryGetTarget(out var value))
            {
                return value;
            }

            value = getValue();
            if (created)
            {
                cache.SetTarget(value);
            }
            else
            {
                cache = new WeakReference<T>(value);
            }

            return value;
        }
    }
}
