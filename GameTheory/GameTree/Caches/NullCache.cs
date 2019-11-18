// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree.Caches
{
    /// <summary>
    /// A null-object cache, meaning a cache that does not store any items and implements the interface by returning default values.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states in the cache.</typeparam>
    /// <typeparam name="TMove">The type of moves supported by the game states in the cache.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    public class NullCache<TGameState, TMove, TScore> : IGameStateCache<TGameState, TMove, TScore>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <inheritdoc/>
        public void SetValue(TGameState state, StateNode<TGameState, TMove, TScore> result)
        {
        }

        /// <inheritdoc/>
        public void Trim()
        {
        }

        /// <inheritdoc/>
        public bool TryGetValue(TGameState state, out StateNode<TGameState, TMove, TScore> cached)
        {
            cached = default;
            return false;
        }
    }
}
