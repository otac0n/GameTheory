// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree
{
    /// <summary>
    /// Represents a cache, primarily for use as a transposition table.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states in the cache.</typeparam>
    /// <typeparam name="TMove">The type of moves supported by the game states in the cache.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    public interface IGameStateCache<TGameState, TMove, TScore>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Overwrites an item or adds an item into the cache.
        /// </summary>
        /// <param name="state">The game state used as the key.</param>
        /// <param name="result">The mainline used as the value.</param>
        void SetValue(TGameState state, StateNode<TGameState, TMove, TScore> result);

        /// <summary>
        /// Instructs the cache to free available memory.
        /// </summary>
        void Trim();

        /// <summary>
        /// Searches the cache for the specified item and returns the value if found.
        /// </summary>
        /// <param name="state">The game state used as the key.</param>
        /// <param name="cached">The mainline value stored in the cache.</param>
        /// <returns><c>true</c>, if the item was found in the cache; <c>false</c>, otherwise.</returns>
        bool TryGetValue(TGameState state, out StateNode<TGameState, TMove, TScore> cached);
    }
}
