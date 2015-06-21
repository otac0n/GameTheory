namespace GameTheory
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Linq;

    public class EnumCollection<TEnum> : IEnumerable<TEnum>, IReadOnlyList<TEnum> where TEnum : struct
    {
        public static readonly EnumCollection<TEnum> Empty;
        private static readonly int Capacity;
        private readonly int count;
        private readonly ImmutableList<int> storage;

        static EnumCollection()
        {
            Capacity = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(x => Convert.ToInt32(x)).Max() + 1;
            Empty = new EnumCollection<TEnum>(new TEnum[0]);
        }

        public EnumCollection()
        {
            this.count = 0;
            this.storage = ImmutableList.Create(new int[Capacity]);
        }

        public EnumCollection(params TEnum[] items)
            : this(items.AsEnumerable())
        {
        }

        public EnumCollection(IEnumerable<TEnum> items)
        {
            var count = 0;
            var storage = new int[Capacity];
            foreach (var item in items)
            {
                var key = Convert.ToInt32(item);

                checked
                {
                    count++;
                }

                storage[key]++;
            }

            this.count = count;
            this.storage = ImmutableList.Create(storage);
        }

        private EnumCollection(int count, ImmutableList<int> storage)
        {
            this.count = count;
            this.storage = storage;
        }

        public int Count
        {
            get { return this.count; }
        }

        public IEnumerable<TEnum> Keys
        {
            get { return Enumerable.Range(0, Capacity).Where(i => this.storage[i] > 0).Select(i => (TEnum)Enum.ToObject(typeof(TEnum), i)); }
        }

        TEnum IReadOnlyList<TEnum>.this[int index]
        {
            get
            {
                Contract.Requires(index >= 0 && index < this.count);

                for (var i = 0; i < Capacity; i++)
                {
                    index -= this.storage[i];
                    if (index < 0) return (TEnum)Enum.ToObject(typeof(TEnum), i);
                }

                throw new IndexOutOfRangeException("index");
            }
        }

        public int this[TEnum item]
        {
            get
            {
                var key = Convert.ToInt32(item);
                return this.storage[key];
            }
        }

        public EnumCollection<TEnum> Add(TEnum item)
        {
            var key = Convert.ToInt32(item);
            return new EnumCollection<TEnum>(checked(this.count + 1), this.storage.SetItem(key, this.storage[key] + 1));
        }

        public EnumCollection<TEnum> Add(TEnum item, int count)
        {
            Contract.Requires(count >= 1);
            var key = Convert.ToInt32(item);
            return new EnumCollection<TEnum>(checked(this.count + count), this.storage.SetItem(key, this.storage[key] + count));
        }

        public EnumCollection<TEnum> AddRange(IEnumerable<TEnum> items)
        {
            return this.AddRange(new EnumCollection<TEnum>(items));
        }

        public EnumCollection<TEnum> AddRange(EnumCollection<TEnum> items)
        {
            var count = checked(this.count + items.count);
            var storage = new int[Capacity];
            for (var i = 0; i < Capacity; i++)
            {
                storage[i] = this.storage[i] + items.storage[i];
            }

            return new EnumCollection<TEnum>(count, ImmutableList.Create(storage));
        }

        public IEnumerable<EnumCollection<TEnum>> Combinations(int count, bool includeSmaller = false)
        {
            if (count <= 0) yield break;
            if (count > this.count) yield break;

            var storage = new int[Capacity];
            var digitalSum = 0;

            Func<int, bool> increment = null;
            increment = i =>
            {
                if (i >= Capacity) return true;

                var digit = ++storage[i];
                digitalSum++;

                if (digitalSum > count || digit > Math.Min(count, this.storage[i]))
                {
                    storage[i] = 0;
                    digitalSum -= digit;
                    return increment(i + 1);
                }

                return false;
            };

            while (!increment(0))
            {
                if (includeSmaller || digitalSum == count) yield return new EnumCollection<TEnum>(count, ImmutableList.Create(storage));
            }
        }

        public IEnumerator<TEnum> GetEnumerator()
        {
            for (var i = 0; i < Capacity; i++)
            {
                var repeat = this.storage[i];
                while (repeat-- > 0) yield return (TEnum)Enum.ToObject(typeof(TEnum), i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public EnumCollection<TEnum> Remove(TEnum item)
        {
            var key = Convert.ToInt32(item);
            var count = this.storage[key];
            return count > 0 ? new EnumCollection<TEnum>(this.count - 1, this.storage.SetItem(key, count - 1)) : this;
        }

        public EnumCollection<TEnum> Remove(TEnum item, int count)
        {
            Contract.Requires(count >= 1);
            var key = Convert.ToInt32(item);
            var existing = this.storage[key];
            count = Math.Min(count, existing);
            return count > 0 ? new EnumCollection<TEnum>(this.count - count, this.storage.SetItem(key, existing - count)) : this;
        }

        public EnumCollection<TEnum> RemoveAll(TEnum item)
        {
            var key = Convert.ToInt32(item);
            var count = this.storage[key];
            return count > 0 ? new EnumCollection<TEnum>(this.count - count, this.storage.SetItem(key, 0)) : this;
        }

        public EnumCollection<TEnum> RemoveAll(Predicate<TEnum> match)
        {
            var count = 0;
            var storage = new int[Capacity];

            for (var i = 0; i < Capacity; i++)
            {
                if (this.storage[i] > 0 && !match((TEnum)Enum.ToObject(typeof(TEnum), i)))
                {
                    count += storage[i] = this.storage[i];
                }
            }

            return count == 0 ? EnumCollection<TEnum>.Empty : new EnumCollection<TEnum>(count, ImmutableList.Create(storage));
        }

        public EnumCollection<TEnum> RemoveRange(IEnumerable<TEnum> items)
        {
            return this.RemoveRange(new EnumCollection<TEnum>(items));
        }

        public EnumCollection<TEnum> RemoveRange(EnumCollection<TEnum> items)
        {
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
    }
}
