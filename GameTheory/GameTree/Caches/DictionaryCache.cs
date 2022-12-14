// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree.Caches
{
    using System.Collections.Generic;

    /// <summary>
    /// A cache for types that overrides <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states in the cache.</typeparam>
    /// <typeparam name="TMove">The type of moves supported by the game states in the cache.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    public class DictionaryCache<TGameState, TMove, TScore> : IGameStateCache<TGameState, TMove, TScore>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        private readonly Dictionary<IGameState<TMove>, StateNode<TGameState, TMove, TScore>> storage = new Dictionary<IGameState<TMove>, StateNode<TGameState, TMove, TScore>>();

        /// <inheritdoc/>
        public void SetValue(TGameState state, StateNode<TGameState, TMove, TScore> result) => this.storage[state] = result;

        /// <inheritdoc/>
        public void Trim() => this.storage.Clear();

        /// <inheritdoc/>
        public bool TryGetValue(TGameState state, out StateNode<TGameState, TMove, TScore> cached) => this.storage.TryGetValue(state, out cached);
    }
}
