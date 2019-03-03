// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Provides a compact collection of enumeration values.
    /// </summary>
    /// <typeparam name="TEnum">The type of enumeration values to store in the collection.</typeparam>
    public class EnumCollection<TEnum> : IEnumerable<TEnum>, IReadOnlyList<TEnum>, IComparable<EnumCollection<TEnum>>, ITokenFormattable
        where TEnum : struct
    {
        private static readonly TEnum[] AllKeys;
        private static readonly int Capacity;
        private readonly int count;
        private readonly ImmutableList<int> storage;

        static EnumCollection()
        {
            Capacity = EnumUtilities<TEnum>.GetValues().Select(x => Convert.ToInt32(x, CultureInfo.InvariantCulture)).Max() + 1;
            AllKeys = Enumerable.Range(0, Capacity).Select(i => (TEnum)Enum.ToObject(typeof(TEnum), i)).ToArray();
            Empty = new EnumCollection<TEnum>(new TEnum[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCollection{TEnum}"/> class with the specified items.
        /// </summary>
        /// <param name="items">The items to add to the collection.</param>
        public EnumCollection(params TEnum[] items)
            : this(items.AsEnumerable())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCollection{TEnum}"/> class with the specified items.
        /// </summary>
        /// <param name="items">The items to add to the collection.</param>
        public EnumCollection(IEnumerable<TEnum> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var count = 0;
            var storage = new int[Capacity];
            foreach (var item in items)
            {
                var key = Convert.ToInt32(item, CultureInfo.InvariantCulture);

                checked
                {
                    count++;
                }

                storage[key]++;
            }

            this.count = count;
            this.storage = ImmutableList.Create(storage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCollection{TEnum}"/> class.
        /// </summary>
        [Obsolete("Use the static Empty property instead.", error: true)]
        public EnumCollection()
        {
            this.count = 0;
            this.storage = ImmutableList.Create(new int[Capacity]);
        }

        private EnumCollection(int count, ImmutableList<int> storage)
        {
            this.count = count;
            this.storage = storage;
        }

        /// <summary>
        /// Gets an empty <see cref="EnumCollection{T}"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This follows the patterns in System.Collections.Immutable")]
        public static EnumCollection<TEnum> Empty { get; }

        /// <inheritdoc />
        public int Count => this.count;

        /// <inheritdoc/>
        public IList<object> FormatTokens
        {
            get
            {
                var tokens = new List<object>();
                for (var i = 0; i < Capacity; i++)
                {
                    var count = this.storage[i];
                    if (count > 0)
                    {
                        if (tokens.Count > 0)
                        {
                            tokens.Add(", ");
                        }

                        if (count > 1)
                        {
                            tokens.Add(count);
                            tokens.Add(SharedResources.Times);
                        }

                        tokens.Add(AllKeys[i]);
                    }
                }

                return tokens;
            }
        }

        /// <summary>
        /// Gets the distinct list of items contained in the collection.
        /// </summary>
        public IEnumerable<TEnum> Keys
        {
            get
            {
                for (var i = 0; i < Capacity; i++)
                {
                    if (this.storage[i] > 0)
                    {
                        yield return AllKeys[i];
                    }
                }
            }
        }

        /// <inheritdoc />
        TEnum IReadOnlyList<TEnum>.this[int index]
        {
            get
            {
                for (var i = 0; i < Capacity; i++)
                {
                    index -= this.storage[i];
                    if (index < 0)
                    {
                        return AllKeys[i];
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the count of the specified item in the collection.
        /// </summary>
        /// <param name="item">The item to find.</param>
        /// <returns>The count of the specified item in the collection.</returns>
        public int this[TEnum item]
        {
            get
            {
                var key = Convert.ToInt32(item, CultureInfo.InvariantCulture);
                return this.storage[key];
            }
        }

        /// <summary>
        /// Makes a copy of the collection and adds the specified item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> Add(TEnum item)
        {
            var key = Convert.ToInt32(item, CultureInfo.InvariantCulture);
            return new EnumCollection<TEnum>(checked(this.count + 1), this.storage.SetItem(key, this.storage[key] + 1));
        }

        /// <summary>
        /// Makes a copy of the collection and adds the specified item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="count">The number of copies of the specified item to add.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> Add(TEnum item, int count)
        {
            if (count == 0)
            {
                return this;
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var key = Convert.ToInt32(item, CultureInfo.InvariantCulture);
            return new EnumCollection<TEnum>(checked(this.count + count), this.storage.SetItem(key, this.storage[key] + count));
        }

        /// <summary>
        /// Makes a copy of the collection and adds the specified items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> AddRange(IEnumerable<TEnum> items)
        {
            return this.AddRange(new EnumCollection<TEnum>(items));
        }

        /// <summary>
        /// Makes a copy of the collection and adds the specified items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> AddRange(EnumCollection<TEnum> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var count = checked(this.count + items.count);
            var storage = new int[Capacity];
            for (var i = 0; i < Capacity; i++)
            {
                storage[i] = this.storage[i] + items.storage[i];
            }

            return new EnumCollection<TEnum>(count, ImmutableList.Create(storage));
        }

        /// <summary>
        /// Gets an enumerable collection containing all combinations of the items in this collection.
        /// </summary>
        /// <param name="count">The size of the combinations.</param>
        /// <param name="includeSmaller">A value indicating whether or not smaller combinations should also be returned.</param>
        /// <returns>An enumerable collection of combinations.</returns>
        public IEnumerable<EnumCollection<TEnum>> Combinations(int count, bool includeSmaller = false)
        {
            if (count <= 0)
            {
                yield break;
            }

            if (!includeSmaller && count > this.count)
            {
                yield break;
            }

            var storage = new int[Capacity];
            var digitalSum = 0;

            bool increment(int i)
            {
                if (i >= Capacity)
                {
                    return true;
                }

                var digit = ++storage[i];
                digitalSum++;

                if (digitalSum > count || digit > Math.Min(count, this.storage[i]))
                {
                    storage[i] = 0;
                    digitalSum -= digit;
                    return increment(i + 1);
                }

                return false;
            }

            while (!increment(0))
            {
                if (includeSmaller || digitalSum == count)
                {
                    yield return new EnumCollection<TEnum>(digitalSum, ImmutableList.Create(storage));
                }
            }
        }

        /// <inheritdoc/>
        public int CompareTo(EnumCollection<TEnum> other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int comp;

            if ((comp = this.count.CompareTo(other.count)) != 0)
            {
                return comp;
            }

            for (var i = 0; i < Capacity; i++)
            {
                if ((comp = this.storage[i].CompareTo(other.storage[i])) != 0)
                {
                    return comp;
                }
            }

            return 0;
        }

        /// <inheritdoc />
        public IEnumerator<TEnum> GetEnumerator()
        {
            for (var i = 0; i < Capacity; i++)
            {
                var repeat = this.storage[i];
                while (repeat-- > 0)
                {
                    yield return AllKeys[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Makes a copy of the collection and removes the specified item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> Remove(TEnum item)
        {
            var key = Convert.ToInt32(item, CultureInfo.InvariantCulture);
            var count = this.storage[key];
            return count > 0 ? new EnumCollection<TEnum>(this.count - 1, this.storage.SetItem(key, count - 1)) : this;
        }

        /// <summary>
        /// Makes a copy of the collection and removes the specified item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="count">The number of copies of the specified item to add.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> Remove(TEnum item, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var key = Convert.ToInt32(item, CultureInfo.InvariantCulture);
            var existing = this.storage[key];
            count = Math.Min(count, existing);
            return count > 0 ? new EnumCollection<TEnum>(this.count - count, this.storage.SetItem(key, existing - count)) : this;
        }

        /// <summary>
        /// Makes a copy of the collection and removes all copies of the specified item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> RemoveAll(TEnum item)
        {
            var key = Convert.ToInt32(item, CultureInfo.InvariantCulture);
            var count = this.storage[key];
            return count > 0 ? new EnumCollection<TEnum>(this.count - count, this.storage.SetItem(key, 0)) : this;
        }

        /// <summary>
        /// Makes a copy of the collection and removes items that match the specified predicate from the collection.
        /// </summary>
        /// <param name="match">A predicate indicating which items to remove.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> RemoveAll(Predicate<TEnum> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var count = 0;
            var storage = new int[Capacity];

            for (var i = 0; i < Capacity; i++)
            {
                if (this.storage[i] > 0 && !match(AllKeys[i]))
                {
                    count += storage[i] = this.storage[i];
                }
            }

            return count == 0 ? EnumCollection<TEnum>.Empty : new EnumCollection<TEnum>(count, ImmutableList.Create(storage));
        }

        /// <summary>
        /// Makes a copy of the collection and removes the specified items from the collection.
        /// </summary>
        /// <param name="items">The items to remove.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> RemoveRange(IEnumerable<TEnum> items)
        {
            return this.RemoveRange(new EnumCollection<TEnum>(items));
        }

        /// <summary>
        /// Makes a copy of the collection and removes the specified items from the collection.
        /// </summary>
        /// <param name="items">The items to remove.</param>
        /// <returns>The new collection.</returns>
        public EnumCollection<TEnum> RemoveRange(EnumCollection<TEnum> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var count = this.count;
            var storage = new int[Capacity];
            for (var i = 0; i < Capacity; i++)
            {
                var remove = Math.Min(this.storage[i], items.storage[i]);
                count -= remove;
                storage[i] = this.storage[i] - remove;
            }

            return new EnumCollection<TEnum>(count, ImmutableList.Create(storage));
        }

        /// <inheritdoc />
        public override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
