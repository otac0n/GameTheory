// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GameTheory.Comparers;

    /// <summary>
    /// Provides methods to facilitate Monte-Carlo approaches to exploring game state variations.
    /// </summary>
    /// <typeparam name="TState">The type of gamestate being shuffled.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This type implements IEnumerable for convenience of interface, but is not semantically a collection.")]
    public class GameShuffler<TState> : IEnumerable<TState>
    {
        private readonly TState startingState;
        private readonly Dictionary<Tuple<string, Type>, IGrouping> storage = new Dictionary<Tuple<string, Type>, IGrouping>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GameShuffler{TState}"/> class.
        /// </summary>
        /// <param name="startingState">The state that will be shuffled.</param>
        public GameShuffler(TState startingState)
        {
            this.startingState = startingState;
        }

        private interface IGrouping
        {
            TState Shuffle(TState state);
        }

        /// <summary>
        /// Adds a shuffleable item to the collection in a default group.
        /// </summary>
        /// <remarks>
        /// Each type is a distinct group, so you may wish to explicitly specify a less specific type.
        /// </remarks>
        /// <typeparam name="TItem">The type of the item being added.</typeparam>
        /// <param name="item">The item that can be shuffled.</param>
        /// <param name="setValue">A function that will overwrite the item with another item in the same group.</param>
        /// <param name="id">An object that may be used by constraints to identify this source.</param>
        public void Add<TItem>(TItem item, Func<TState, TItem, TState> setValue, object id = null) => this.Add(item, setValue, id);

        /// <summary>
        /// Adds a shuffleable item to the collection in the specified group.
        /// </summary>
        /// <remarks>
        /// Each type is a distinct group, so you may wish to explicitly specify a less specific type.
        /// </remarks>
        /// <typeparam name="TItem">The type of the item being added.</typeparam>
        /// <param name="group">The group of items that may be shuffled.</param>
        /// <param name="item">The item that can be shuffled.</param>
        /// <param name="setValue">A function that will overwrite the item with another item in the same group.</param>
        /// <param name="id">An object that may be used by constraints to identify this source.</param>
        public void Add<TItem>(string group, TItem item, Func<TState, TItem, TState> setValue, object id = null)
        {
            var grouping = this.GetOrAddGrouping<TItem>(group);

            grouping.AddAccessor(new Accessor<TItem>(id, item, setValue));
        }

        /// <summary>
        /// Adds a shuffleable item to the collection in a default group.
        /// </summary>
        /// <remarks>
        /// Each type is a distinct group, so you may wish to explicitly specify a less specific type.
        /// </remarks>
        /// <typeparam name="TItem">The type of the item being added.</typeparam>
        /// <param name="items">The collection of items that can be shuffled.</param>
        /// <param name="setItems">A function that will overwrite the collection with another collection of items in the same group of the same size.</param>
        /// <param name="id">An object that may be used by constraints to identify this source.</param>
        public void AddCollection<TItem>(IReadOnlyList<TItem> items, Func<TState, IReadOnlyList<TItem>, TState> setItems, object id = null) => this.Add(null, items, setItems, id);

        /// <summary>
        /// Adds a shuffleable item to the collection in the specified group.
        /// </summary>
        /// <remarks>
        /// Each type is a distinct group, so you may wish to explicitly specify a less specific type.
        /// </remarks>
        /// <typeparam name="TItem">The type of the item being added.</typeparam>
        /// <param name="group">The group of items that may be shuffled.</param>
        /// <param name="items">The collection of items that can be shuffled.</param>
        /// <param name="setItems">A function that will overwrite the collection with another collection of items in the same group of the same size.</param>
        /// <param name="id">An object that may be used by constraints to identify this source.</param>
        public void AddCollection<TItem>(string group, IReadOnlyList<TItem> items, Func<TState, IReadOnlyList<TItem>, TState> setItems, object id = null)
        {
            var grouping = this.GetOrAddGrouping<TItem>(group);

            grouping.AddAccessor(new Accessor<TItem>(id, items, setItems));
        }

        /// <summary>
        /// Adds a constraint to the specified suffle group.
        /// </summary>
        /// <remarks>
        /// Each type is a distinct group, so you may wish to explicitly specify a less specific type.
        /// </remarks>
        /// <typeparam name="TItem">The type of the item being constrained.</typeparam>
        /// <param name="group">The group of items that may be shuffled.</param>
        /// <param name="constraint">A function that must return <c>true</c> for an item to be shuffled to a specific index.</param>
        public void AddConstraint<TItem>(string group, Func<object, int, TItem, bool> constraint)
        {
            var grouping = this.GetOrAddGrouping<TItem>(group);

            grouping.AddConstraint(constraint);
        }

        /// <inheritdoc/>
        public IEnumerator<TState> GetEnumerator()
        {
            // TODO: For sufficiently small groups, it may be a better choice to yield all possible shuffles in a random order rather than an infinite collection of random shuffles.
            while (true)
            {
                var state = this.startingState;

                foreach (var grouping in this.storage.Values)
                {
                    state = grouping.Shuffle(state);
                }

                yield return state;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private Grouping<TItem> GetOrAddGrouping<TItem>(string group)
        {
            Grouping<TItem> grouping;
            var key = Tuple.Create(group, typeof(TItem));
            if (this.storage.TryGetValue(key, out var value))
            {
                grouping = (Grouping<TItem>)value;
            }
            else
            {
                this.storage[key] = grouping = new Grouping<TItem>();
            }

            return grouping;
        }

        private struct Accessor<TItem>
        {
            public Accessor(object id, TItem item, Func<TState, TItem, TState> setValue)
            {
                this.Id = id;
                this.Items = new[] { item };
                this.SetItems = (state, items) => setValue(state, items.Single());
            }

            public Accessor(object id, IReadOnlyList<TItem> items, Func<TState, IReadOnlyList<TItem>, TState> setItems)
            {
                this.Id = id;
                this.Items = items;
                this.SetItems = setItems;
            }

            public object Id { get; }

            public IReadOnlyList<TItem> Items { get; }

            public Func<TState, IReadOnlyList<TItem>, TState> SetItems { get; }
        }

        private class Grouping<TItem> : IGrouping
        {
            private static readonly ValueListComparer<int> ListComparer = new ValueListComparer<int>();
            private readonly List<Func<object, int, TItem, bool>> constraints = new List<Func<object, int, TItem, bool>>();
            private readonly List<Accessor<TItem>> storage = new List<Accessor<TItem>>();

            public void AddAccessor(Accessor<TItem> accessor) => this.storage.Add(accessor);

            public void AddConstraint(Func<object, int, TItem, bool> constraint) => this.constraints.Add(constraint);

            public TState Shuffle(TState state)
            {
                var simpleShuffle = true;
                var items = this.storage.SelectMany(a => a.Items).ToList();
                if (this.constraints.Count != 0)
                {
                    bool AggregateConstraint(object id, int index, TItem value) => this.constraints.All(c => c(id, index, value));

                    var count = items.Count;
                    var pairwise = new bool[count, count];
                    List<int>[] rows = null;
                    List<int>[] cols = null;

                    var offset = 0;
                    foreach (var accessor in this.storage)
                    {
                        var a = accessor.Items;
                        var ct = a.Count;

                        for (var i = 0; i < ct; i++)
                        {
                            var row = i + offset;
                            for (var col = 0; col < count; col++)
                            {
                                if (pairwise[row, col] = AggregateConstraint(accessor.Id, i, items[col]))
                                {
                                    if (!simpleShuffle)
                                    {
                                        rows[row].Add(col);
                                        cols[col].Add(row);
                                    }
                                }
                                else if (simpleShuffle)
                                {
                                    simpleShuffle = false;

                                    rows = new List<int>[count];
                                    cols = new List<int>[count];
                                    for (var j = 0; j < count; j++)
                                    {
                                        if (j < row)
                                        {
                                            rows[j] = new List<int>(j == 0 ? Enumerable.Range(0, count) : rows[0]);
                                        }
                                        else if (j == row)
                                        {
                                            rows[j] = new List<int>(count);
                                            rows[j].AddRange(Enumerable.Range(0, col));
                                        }
                                        else
                                        {
                                            rows[j] = new List<int>(count);
                                        }

                                        cols[j] = new List<int>(count);
                                        cols[j].AddRange(Enumerable.Range(0, j < col ? row + 1 : row));
                                    }
                                }
                            }
                        }

                        offset += ct;
                    }

                    if (!simpleShuffle)
                    {
                        var results = new List<TItem>(new TItem[count]);

                        var remainingRows = new HashSet<int>(Enumerable.Range(0, count));
                        var remainingCols = new HashSet<int>(remainingRows);
                        var dimensions = new[]
                        {
                            new { remaining = remainingRows, otherRemaining = remainingCols, source = rows, other = cols, set = new Action<int, int>((int id, int selected) => results[id] = items[selected]) },
                            new { remaining = remainingCols, otherRemaining = remainingRows, source = cols, other = rows, set = new Action<int, int>((int id, int selected) => results[selected] = items[id]) },
                        };

                        while (remainingRows.Count > 0 || remainingCols.Count > 0)
                        {
                            var minGroup = dimensions.Select(d =>
                            {
                                var minimizing = d.remaining
                                    .AllMinBy(r => d.source[r].Count, out var minOpposite)
                                    .GroupBy(r => d.source[r], ListComparer)
                                    .MaxBy(g => g.Count())
                                    .First();
                                return new { dimension = d, minimizing, minOpposite };
                            }).MinBy(d => d.minOpposite);

                            if (minGroup.minOpposite == 0)
                            {
                                throw new InvalidOperationException("The constraints describe a state with no satisfying permutation.");
                            }

                            var id = minGroup.minimizing;
                            var selected = minGroup.dimension.source[id].Pick();
                            var dimension = minGroup.dimension;
                            dimension.set(id, selected);
                            dimension.remaining.Remove(id);
                            dimension.otherRemaining.Remove(selected);
                            dimension.source[id] = null;
                            dimension.other[selected] = null;

                            for (var i = 0; i < count; i++)
                            {
                                if (dimension.remaining.Contains(i))
                                {
                                    dimension.source[i].Remove(selected);
                                }

                                if (dimension.otherRemaining.Contains(i))
                                {
                                    dimension.other[i].Remove(id);
                                }
                            }
                        }

                        items = results;
                    }
                }

                if (simpleShuffle)
                {
                    items.ShuffleInPlace();
                }

                var sourceIndex = 0;
                foreach (var accessor in this.storage)
                {
                    var range = items.GetRange(sourceIndex, accessor.Items.Count);
                    state = accessor.SetItems(state, range);
                    sourceIndex += accessor.Items.Count;
                }

                return state;
            }
        }
    }
}
