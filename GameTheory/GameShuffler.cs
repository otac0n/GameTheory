// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

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
        public void Add<TItem>(TItem item, Func<TState, TItem, TState> setValue) => this.Add(null, item, setValue);

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
        public void Add<TItem>(string group, TItem item, Func<TState, TItem, TState> setValue)
        {
            var grouping = this.GetOrAddGrouping<TItem>(group);

            grouping.Add(new Accessor<TItem>(item, setValue));
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
        public void AddCollection<TItem>(IReadOnlyCollection<TItem> items, Func<TState, IReadOnlyCollection<TItem>, TState> setItems) => this.Add(null, items, setItems);

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
        public void AddCollection<TItem>(string group, IReadOnlyCollection<TItem> items, Func<TState, IReadOnlyCollection<TItem>, TState> setItems)
        {
            var grouping = this.GetOrAddGrouping<TItem>(group);

            grouping.Add(new Accessor<TItem>(items, setItems));
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
                value = this.storage[key] = grouping = new Grouping<TItem>();
            }

            return grouping;
        }

        private struct Accessor<TItem>
        {
            public Accessor(TItem item, Func<TState, TItem, TState> setValue)
            {
                this.Items = new[] { item };
                this.SetItems = (state, items) => setValue(state, items.Single());
            }

            public Accessor(IReadOnlyCollection<TItem> items, Func<TState, IReadOnlyCollection<TItem>, TState> setItems)
            {
                this.Items = items;
                this.SetItems = setItems;
            }

            public IReadOnlyCollection<TItem> Items { get; }

            public Func<TState, IReadOnlyCollection<TItem>, TState> SetItems { get; }
        }

        private class Grouping<TItem> : List<Accessor<TItem>>, IGrouping
        {
            public TState Shuffle(TState state)
            {
                var items = this.SelectMany(a => a.Items).Shuffle();
                var sourceIndex = 0;
                foreach (var accessor in this)
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
