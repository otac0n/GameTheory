// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Provides a thread-static instance of the <see cref="System.Random"/> class.
    /// </summary>
    public static class Random
    {
        private static int counter;

        [ThreadStatic]
        private static System.Random instance;

        /// <summary>
        /// Gets an instance of the <see cref="System.Random"/> class that is unique to the current thread.
        /// </summary>
        /// <remarks>
        /// Do not allow this instance to be observed by threads.
        /// </remarks>
        public static System.Random Instance =>
            instance ?? (instance = new System.Random(unchecked(Environment.TickCount * Interlocked.Increment(ref counter))));

        /// <summary>
        /// Deals items from a collection, using a discard pile to replenish the deck if necessary.
        /// </summary>
        /// <typeparam name="T">The type of items in the deck.</typeparam>
        /// <param name="deck">The source of items being dealt.</param>
        /// <param name="count">The number of items to deal.</param>
        /// <param name="dealt">The resulting items.</param>
        /// <param name="discards">The discards pile.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static ImmutableList<T> Deal<T>(this ImmutableList<T> deck, int count, out ImmutableList<T> dealt, ref ImmutableList<T> discards, System.Random instance = null)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            if (discards == null)
            {
                throw new ArgumentNullException(nameof(discards));
            }

            var allDealt = ImmutableList<T>.Empty;
            deck = deck.Deal(count, out var newlyDealt, instance);
            allDealt = allDealt.AddRange(newlyDealt);

            count -= allDealt.Count;
            Debug.Assert(count == 0 || deck.Count == 0, "Expected either an empty deck or no cards left to deal.");

            if (count > 0 && discards.Count > 0)
            {
                deck = discards;
                discards = ImmutableList<T>.Empty;

                deck = deck.Deal(count, out newlyDealt, instance);
                allDealt = allDealt.AddRange(newlyDealt);
            }

            dealt = allDealt;
            return deck;
        }

        /// <summary>
        /// Deals items from a collection, using a discard pile to replenish the deck if necessary.
        /// </summary>
        /// <typeparam name="T">The type of items in the deck.</typeparam>
        /// <param name="deck">The source of items being dealt.</param>
        /// <param name="count">The number of items to deal.</param>
        /// <param name="dealt">The resulting items.</param>
        /// <param name="discards">The discards pile.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static EnumCollection<T> Deal<T>(this EnumCollection<T> deck, int count, out ImmutableList<T> dealt, ref EnumCollection<T> discards, System.Random instance = null)
            where T : struct
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            if (discards == null)
            {
                throw new ArgumentNullException(nameof(discards));
            }

            var allDealt = ImmutableList<T>.Empty;
            deck = deck.Deal(count, out var newlyDealt, instance);
            allDealt = allDealt.AddRange(newlyDealt);

            count -= allDealt.Count;
            Debug.Assert(count == 0 || deck.Count == 0, "Expected either an empty deck or no cards left to deal.");

            if (count > 0 && discards.Count > 0)
            {
                deck = discards;
                discards = EnumCollection<T>.Empty;

                deck = deck.Deal(count, out newlyDealt, instance);
                allDealt = allDealt.AddRange(newlyDealt);
            }

            dealt = allDealt;
            return deck;
        }

        /// <summary>
        /// Deals items from a collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the deck.</typeparam>
        /// <param name="deck">The source of items being dealt.</param>
        /// <param name="count">The number of items to deal.</param>
        /// <param name="dealt">The resulting items.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static ImmutableList<T> Deal<T>(this ImmutableList<T> deck, int count, out ImmutableList<T> dealt, System.Random instance = null)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            instance = instance ?? Instance;

            if (count >= deck.Count)
            {
                dealt = deck.Shuffle(instance).ToImmutableList();
                return ImmutableList<T>.Empty;
            }

            var dealtBuilder = ImmutableList.CreateBuilder<T>();
            for (; count >= 1; count--)
            {
                var ix = instance.Next(deck.Count);
                dealtBuilder.Add(deck[ix]);
                deck = deck.RemoveAt(ix);
            }

            dealt = dealtBuilder.ToImmutable();
            return deck;
        }

        /// <summary>
        /// Deals items from a collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the deck.</typeparam>
        /// <param name="deck">The source of items being dealt.</param>
        /// <param name="count">The number of items to deal.</param>
        /// <param name="dealt">The resulting items.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static EnumCollection<T> Deal<T>(this EnumCollection<T> deck, int count, out ImmutableList<T> dealt, System.Random instance = null)
            where T : struct
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            instance = instance ?? Instance;

            if (count >= deck.Count)
            {
                dealt = deck.Shuffle(instance).ToImmutableList();
                return EnumCollection<T>.Empty;
            }

            var dealtBuilder = ImmutableList.CreateBuilder<T>();
            for (; count >= 1; count--)
            {
                var pick = deck.Pick();
                dealtBuilder.Add(pick);
                deck = deck.Remove(pick);
            }

            dealt = dealtBuilder.ToImmutable();
            return deck;
        }

        /// <summary>
        /// Deals an item from a collection.  If there are no items remaining, the default value is returned.
        /// </summary>
        /// <typeparam name="T">The type of items in the deck.</typeparam>
        /// <param name="deck">The source of items being dealt.</param>
        /// <param name="dealt">The resulting item or default if there were no items.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static ImmutableList<T> Deal<T>(this ImmutableList<T> deck, out T dealt, System.Random instance = null)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            instance = instance ?? Instance;

            if (deck.Count > 0)
            {
                var ix = instance.Next(deck.Count);
                dealt = deck[ix];
                return deck.RemoveAt(ix);
            }
            else
            {
                dealt = default;
                return deck;
            }
        }

        /// <summary>
        /// Deals an item from a collection.  If there are no items remaining, the default value is returned.
        /// </summary>
        /// <typeparam name="T">The type of items in the deck.</typeparam>
        /// <param name="deck">The source of items being dealt.</param>
        /// <param name="dealt">The resulting item or default if there were no items.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static EnumCollection<T> Deal<T>(this EnumCollection<T> deck, out T dealt, System.Random instance = null)
            where T : struct
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            instance = instance ?? Instance;

            if (deck.Count > 0)
            {
                var ix = instance.Next(deck.Count);

                var value = 0;
                foreach (var key in deck.Keys)
                {
                    value += deck[key];
                    if (value >= ix)
                    {
                        dealt = key;
                        return deck.Remove(key);
                    }
                }

                throw new InvalidOperationException();
            }
            else
            {
                dealt = default;
                return deck;
            }
        }

        /// <summary>
        /// Selects a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IReadOnlyList<T> items, System.Random instance = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            instance = instance ?? Instance;

            return items[instance.Next(items.Count)];
        }

        /// <summary>
        /// Selects a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IList<T> items, System.Random instance = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            instance = instance ?? Instance;

            return items[instance.Next(items.Count)];
        }

        /// <summary>
        /// Selects a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this List<T> items, System.Random instance = null) => Pick((IList<T>)items, instance);

        /// <summary>
        /// Selects a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this T[] items, System.Random instance = null) => Pick((IList<T>)items, instance);

        /// <summary>
        /// Selects a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this ImmutableArray<T> items, System.Random instance = null) => Pick((IList<T>)items, instance);

        /// <summary>
        /// Selects a random element from a collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IEnumerable<T> items, System.Random instance = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            instance = instance ?? Instance;

            var current = default(T);

            var i = 0;
            foreach (var item in items)
            {
                if (instance.Next(i + 1) == 0)
                {
                    current = item;
                }

                i++;
            }

            return current;
        }

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IList<IWeighted<T>> weightedItems, System.Random instance = null)
        {
            if (weightedItems == null)
            {
                throw new ArgumentNullException(nameof(weightedItems));
            }

            instance = instance ?? Instance;

            var totalWeight = weightedItems.Sum(i => i.Weight);
            var threshold = totalWeight * instance.NextDouble();

            var value = 0.0;
            foreach (var item in weightedItems)
            {
                value += item.Weight;
                if (value >= threshold)
                {
                    return item.Value;
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IList<Weighted<T>> weightedItems, System.Random instance = null)
        {
            if (weightedItems == null)
            {
                throw new ArgumentNullException(nameof(weightedItems));
            }

            instance = instance ?? Instance;

            var totalWeight = weightedItems.Sum(i => i.Weight);
            var threshold = totalWeight * instance.NextDouble();

            var value = 0.0;
            foreach (var item in weightedItems)
            {
                value += item.Weight;
                if (value >= threshold)
                {
                    return item.Value;
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IReadOnlyList<IWeighted<T>> weightedItems, System.Random instance = null)
        {
            if (weightedItems == null)
            {
                throw new ArgumentNullException(nameof(weightedItems));
            }

            instance = instance ?? Instance;

            var totalWeight = weightedItems.Sum(i => i.Weight);
            var threshold = totalWeight * instance.NextDouble();

            var value = 0.0;
            foreach (var item in weightedItems)
            {
                value += item.Weight;
                if (value >= threshold)
                {
                    return item.Value;
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IReadOnlyList<Weighted<T>> weightedItems, System.Random instance = null)
        {
            if (weightedItems == null)
            {
                throw new ArgumentNullException(nameof(weightedItems));
            }

            instance = instance ?? Instance;

            var totalWeight = weightedItems.Sum(i => i.Weight);
            var threshold = totalWeight * instance.NextDouble();

            var value = 0.0;
            foreach (var item in weightedItems)
            {
                value += item.Weight;
                if (value >= threshold)
                {
                    return item.Value;
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this List<IWeighted<T>> weightedItems, System.Random instance = null) => Pick((IList<IWeighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this List<Weighted<T>> weightedItems, System.Random instance = null) => Pick((IList<Weighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IWeighted<T>[] weightedItems, System.Random instance = null) => Pick((IList<IWeighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this Weighted<T>[] weightedItems, System.Random instance = null) => Pick((IList<Weighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this ImmutableArray<IWeighted<T>> weightedItems, System.Random instance = null) => Pick((IList<IWeighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this ImmutableArray<Weighted<T>> weightedItems, System.Random instance = null) => Pick((IList<Weighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a collection of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IEnumerable<IWeighted<T>> weightedItems, System.Random instance = null)
        {
            return weightedItems.ToList().Pick(instance);
        }

        /// <summary>
        /// Shuffles a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The items to shuffle.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        /// <returns>A new list containing the original items in a new order.</returns>
        public static List<T> Shuffle<T>(this IEnumerable<T> source, System.Random instance = null)
        {
            var copy = source.ToList();
            copy.ShuffleInPlace(instance);
            return copy;
        }

        /// <summary>
        /// Shuffles a collection in place.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The items to shuffle.</param>
        /// <param name="instance">An instance of <see cref="System.Random"/> to use.</param>
        public static void ShuffleInPlace<T>(this IList<T> source, System.Random instance = null)
        {
            instance = instance ?? Instance;

            for (var i = source.Count - 1; i >= 1; i--)
            {
                var j = instance.Next(i + 1);
                (source[i], source[j]) = (source[j], source[i]);
            }
        }
    }
}
