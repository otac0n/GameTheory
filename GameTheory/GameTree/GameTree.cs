// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree
{
    /// <summary>
    /// Provides a persistent view of a the evolution of a <see cref="IGameState{TMove}"/> across move variations.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states in the tree.</typeparam>
    /// <typeparam name="TMove">The type of moves supported by the game states in the tree.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    public class GameTree<TGameState, TMove, TScore>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        private readonly IGameStateCache<TGameState, TMove, TScore> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTree{TGameState, TMove, TScore}"/> class.
        /// </summary>
        /// <param name="cache">The <see cref="IGameStateCache{TGameState, TMove, TScore}"/> to be used as a transposition table.</param>
        public GameTree(IGameStateCache<TGameState, TMove, TScore> cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// Gets the node associated with the specified game state.
        /// </summary>
        /// <param name="value">The game state to look up or store.</param>
        /// <returns>A node representing the specified state.</returns>
        public StateNode<TGameState, TMove, TScore> GetOrAdd(TGameState value)
        {
            if (!this.cache.TryGetValue(value, out var result))
            {
                this.cache.SetValue(value, result = new StateNode<TGameState, TMove, TScore>(this, value));
            }

            return result;
        }

        /// <summary>
        /// Instructs the tree to free available memory.
        /// </summary>
        public void Trim() => this.cache.Trim();
    }
}
