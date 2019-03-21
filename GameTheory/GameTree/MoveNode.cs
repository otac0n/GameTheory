// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in a <see cref="GameTree{TMove, TScore}"/>.
    /// </summary>
    /// <typeparam name="TMove">The type of moves supported by the game states.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    public class MoveNode<TMove, TScore>
        where TMove : IMove
    {
        internal MoveNode(GameTree<TMove, TScore> tree, IGameState<TMove> state, TMove move)
        {
            this.Move = move;
            this.Outcomes = state.GetOutcomes(move).Select(o => new Weighted<StateNode<TMove, TScore>>(tree.GetOrAdd(o.Value), o.Weight)).ToArray();
        }

        /// <summary>
        /// Gets the move that this node represents.
        /// </summary>
        public TMove Move { get; }

        /// <summary>
        /// Gets the outcomes from this move.
        /// </summary>
        /// <remarks>
        /// For performance, the same array reference is returned each time the property is accessed.
        /// </remarks>
        public IReadOnlyList<Weighted<StateNode<TMove, TScore>>> Outcomes { get; }

        /// <summary>
        /// Gets or sets the score assigned to this move.
        /// </summary>
        public TScore Score { get; set; }
    }
}
