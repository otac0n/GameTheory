// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree.Caches
{
    using System;

    /// <summary>
    /// A cache for types that implements <see cref="IComparable{T}"/>.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states in the cache.</typeparam>
    /// <typeparam name="TMove">The type of moves supported by the game states in the cache.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    public class SplayTreeCache<TGameState, TMove, TScore> : IGameStateCache<TGameState, TMove, TScore>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        private const int TrimDepth = 32;

        private readonly SplayTreeDictionary<IGameState<TMove>, StateNode<TGameState, TMove, TScore>> storage = new SplayTreeDictionary<IGameState<TMove>, StateNode<TGameState, TMove, TScore>>();

        /// <inheritdoc/>
        public void SetValue(TGameState state, StateNode<TGameState, TMove, TScore> result) => this.storage[state] = result;

        /// <inheritdoc/>
        public void Trim() => this.storage.Trim(TrimDepth);

        /// <inheritdoc/>
        public bool TryGetValue(TGameState state, out StateNode<TGameState, TMove, TScore> cached) => this.storage.TryGetValue(state, out cached);
    }
}
