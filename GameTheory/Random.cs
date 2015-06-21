// -----------------------------------------------------------------------
// <copyright file="Random.cs" company="(none)">
//   Copyright © 2014 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

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
        [ThreadStatic]
        private static System.Random instance;

        /// <summary>
        /// Gets an instance of the <see cref="System.Random"/> class that is unique to the current thread.
        /// </summary>
        /// <remarks>
        /// Do not allow this instance to be used on other threads.
        /// </remarks>
        public static System.Random Instance
        {
            get { return instance ?? (instance = new System.Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId)); }
        }

        public static ImmutableList<T> Deal<T>(this ImmutableList<T> deck, int count, out ImmutableList<T> dealt, ref ImmutableList<T> discards, System.Random instance = null)
        {
            var allDealt = ImmutableList<T>.Empty;
            ImmutableList<T> newlyDealt;
            deck = deck.Deal(count, out newlyDealt, instance);
            allDealt = allDealt.AddRange(newlyDealt);

            count -= allDealt.Count;
            Debug.Assert(count == 0 || deck.Count == 0);

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

        public static EnumCollection<T> Deal<T>(this EnumCollection<T> deck, int count, out ImmutableList<T> dealt, ref EnumCollection<T> discards, System.Random instance = null) where T : struct
        {
            var allDealt = ImmutableList<T>.Empty;
            ImmutableList<T> newlyDealt;
            deck = deck.Deal(count, out newlyDealt, instance);
            allDealt = allDealt.AddRange(newlyDealt);

            count -= allDealt.Count;
            Debug.Assert(count == 0 || deck.Count == 0);

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

        public static ImmutableList<T> Deal<T>(this ImmutableList<T> deck, int count, out ImmutableList<T> dealt, System.Random instance = null)
        {
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

        public static EnumCollection<T> Deal<T>(this EnumCollection<T> deck, int count, out ImmutableList<T> dealt, System.Random instance = null) where T : struct
        {
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

        public static T Pick<T>(this IReadOnlyList<T> items, System.Random instance = null)
        {
            instance = instance ?? Instance;

            return items[instance.Next(items.Count)];
        }

        public static T Pick<T>(this IList<T> items, System.Random instance = null)
        {
            instance = instance ?? Instance;

            return items[instance.Next(items.Count)];
        }

        public static T Pick<T>(this IEnumerable<T> items, System.Random instance = null)
        {
            instance = instance ?? Instance;

            T current = default(T);

            var i = 0;
            foreach (var item in items)
            {
                if (instance.Next(i + 1) == 0) current = item;
                i++;
            }

            return current;
        }

        public static List<T> Shuffle<T>(this IEnumerable<T> source, System.Random instance = null)
        {
            instance = instance ?? Instance;

            var copy = source.ToList();
            for (var i = copy.Count - 1; i >= 1; i--)
            {
                var j = instance.Next(i + 1);
                var swap = copy[j];
                copy[j] = copy[i];
                copy[i] = swap;
            }

            return copy;
        }
    }
}
