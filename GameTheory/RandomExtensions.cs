// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Provides a thread-static instance of the <see cref="Random"/> class.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Returns a random floating point number with the specified normal distribution.
        /// </summary>
        /// <param name="instance">An underlying <see cref="Random"/> instance to use for generation of values.</param>
        /// <param name="mean">The mean value of the distribution.</param>
        /// <param name="variance">The variance of the distribution.</param>
        /// <returns>A double precision floating point number from the specified normal distribution.</returns>
        public static double NextDoubleNormal(this Random instance, double mean, double variance)
        {
            var value = instance.NextDoubleNormal();
            return value * variance + mean;
        }

        /// <summary>
        /// Returns a random floating point number with a standard normal distribution.
        /// </summary>
        /// <param name="instance">An underlying <see cref="Random"/> instance to use for generation of values.</param>
        /// <returns>A double precision floating point number from the standard normal distribution.</returns>
        public static double NextDoubleNormal(this Random instance)
        {
            double u1, u2;
            do
            {
                u1 = instance.NextDouble();
                u2 = instance.NextDouble();
            }
            while (u1 <= double.Epsilon);

            return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
        }

        /// <summary>
        /// Deals items from a collection, using a discard pile to replenish the deck if necessary.
        /// </summary>
        /// <typeparam name="T">The type of items in the deck.</typeparam>
        /// <param name="deck">The source of items being dealt.</param>
        /// <param name="count">The number of items to deal.</param>
        /// <param name="dealt">The resulting items.</param>
        /// <param name="discards">The discards pile.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static ImmutableList<T> Deal<T>(this ImmutableList<T> deck, int count, out ImmutableList<T> dealt, ref ImmutableList<T> discards, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(deck);
            ArgumentNullException.ThrowIfNull(discards);

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static EnumCollection<T> Deal<T>(this EnumCollection<T> deck, int count, out ImmutableList<T> dealt, ref EnumCollection<T> discards, Random instance = null)
            where T : struct
        {
            ArgumentNullException.ThrowIfNull(deck);
            ArgumentNullException.ThrowIfNull(discards);

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static ImmutableList<T> Deal<T>(this ImmutableList<T> deck, int count, out ImmutableList<T> dealt, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(deck);
            instance ??= Random.Shared;

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static EnumCollection<T> Deal<T>(this EnumCollection<T> deck, int count, out ImmutableList<T> dealt, Random instance = null)
            where T : struct
        {
            ArgumentNullException.ThrowIfNull(deck);
            instance ??= Random.Shared;

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static ImmutableList<T> Deal<T>(this ImmutableList<T> deck, out T dealt, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(deck);

            instance ??= Random.Shared;

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The remaining deck.</returns>
        public static EnumCollection<T> Deal<T>(this EnumCollection<T> deck, out T dealt, Random instance = null)
            where T : struct
        {
            ArgumentNullException.ThrowIfNull(deck);
            instance ??= Random.Shared;

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IReadOnlyList<T> items, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(items);
            instance ??= Random.Shared;

            return items[instance.Next(items.Count)];
        }

        /// <summary>
        /// Selects a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IList<T> items, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(items);
            instance ??= Random.Shared;

            return items[instance.Next(items.Count)];
        }

        /// <summary>
        /// Selects a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this List<T> items, Random instance = null) => Pick((IList<T>)items, instance);

        /// <summary>
        /// Selects a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this T[] items, Random instance = null) => Pick((IList<T>)items, instance);

        /// <summary>
        /// Selects a random element from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this ImmutableArray<T> items, Random instance = null) => Pick((IList<T>)items, instance);

        /// <summary>
        /// Selects a random element from a collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="items">The items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IEnumerable<T> items, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(items);
            instance ??= Random.Shared;

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IList<IWeighted<T>> weightedItems, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(weightedItems);
            instance ??= Random.Shared;

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IList<Weighted<T>> weightedItems, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(weightedItems);
            instance ??= Random.Shared;

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IReadOnlyList<IWeighted<T>> weightedItems, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(weightedItems);
            instance ??= Random.Shared;

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IReadOnlyList<Weighted<T>> weightedItems, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(weightedItems);
            instance ??= Random.Shared;

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
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this List<IWeighted<T>> weightedItems, Random instance = null) => Pick((IList<IWeighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this List<Weighted<T>> weightedItems, Random instance = null) => Pick((IList<Weighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IWeighted<T>[] weightedItems, Random instance = null) => Pick((IList<IWeighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this Weighted<T>[] weightedItems, Random instance = null) => Pick((IList<Weighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this ImmutableArray<IWeighted<T>> weightedItems, Random instance = null) => Pick((IList<IWeighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a list of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this ImmutableArray<Weighted<T>> weightedItems, Random instance = null) => Pick((IList<Weighted<T>>)weightedItems, instance);

        /// <summary>
        /// Selects a random element from a collection of weighted items.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="weightedItems">The weighted items to choose from.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>The selected element.</returns>
        public static T Pick<T>(this IEnumerable<IWeighted<T>> weightedItems, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(weightedItems);

            return weightedItems.ToList().Pick(instance);
        }

        /// <summary>
        /// Shuffles a collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The items to shuffle.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        /// <returns>A new list containing the original items in a new order.</returns>
        public static List<T> Shuffle<T>(this IEnumerable<T> source, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(source);

            var copy = source.ToList();
            copy.ShuffleInPlace(instance);
            return copy;
        }

        /// <summary>
        /// Shuffles a collection in place.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The items to shuffle.</param>
        /// <param name="instance">An instance of <see cref="Random"/> to use.</param>
        public static void ShuffleInPlace<T>(this IList<T> source, Random instance = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            instance ??= Random.Shared;

            for (var i = source.Count - 1; i >= 1; i--)
            {
                var j = instance.Next(i + 1);
                (source[i], source[j]) = (source[j], source[i]);
            }
        }
    }
}
