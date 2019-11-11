// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a <see cref="IGameState{TMove}"/> in a <see cref="GameTree{TGameState, TMove, TScore}"/>.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states in the tree.</typeparam>
    /// <typeparam name="TMove">The type of moves supported by the game state.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    public class StateNode<TGameState, TMove, TScore>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        private readonly Dictionary<TMove, MoveNode<TGameState, TMove, TScore>> results = new Dictionary<TMove, MoveNode<TGameState, TMove, TScore>>();
        private readonly GameTree<TGameState, TMove, TScore> tree;
        private TMove[] moves;

        internal StateNode(GameTree<TGameState, TMove, TScore> tree, TGameState state)
        {
            this.tree = tree;
            this.State = state;
        }

        /// <summary>
        /// Gets or sets the mainline for this node.
        /// </summary>
        public Mainline<TGameState, TMove, TScore> Mainline { get; set; }

        /// <summary>
        /// Gets the subsequent moves available from this state.
        /// </summary>
        /// <remarks>
        /// For performance, the same array reference is returned each time the property is accessed.
        /// </remarks>
        public TMove[] Moves => this.moves ?? (this.moves = this.State.GetAvailableMoves().ToArray());

        /// <summary>
        /// Gets the state that this node represents.
        /// </summary>
        public TGameState State { get; }

        /// <summary>
        /// Gets the outcomes of applying the specified move.
        /// </summary>
        /// <param name="move">The move to apply.</param>
        /// <returns>The outcomes of applying <paramref name="move"/> to the game state.</returns>
        public MoveNode<TGameState, TMove, TScore> this[TMove move]
        {
            get
            {
                if (!this.results.TryGetValue(move, out var result))
                {
                    this.results[move] = result = new MoveNode<TGameState, TMove, TScore>(this.tree, this.State, move);
                }

                return result;
            }
        }
    }
}
