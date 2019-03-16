// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System.Collections.Generic;
    using System.Linq;

    public class MoveNode<TMove, TScore>
        where TMove : IMove
    {
        private readonly GameTree<TMove, TScore> tree;

        public MoveNode(GameTree<TMove, TScore> tree, IEnumerable<IWeighted<IGameState<TMove>>> outcomes)
        {
            this.tree = tree;
            this.Outcomes = outcomes.Select(o => new Weighted<StateNode<TMove, TScore>>(this.tree.GetOrAdd(o.Value), o.Weight)).ToArray();
        }

        public TScore Lead { get; set; }

        public IReadOnlyList<Weighted<StateNode<TMove, TScore>>> Outcomes { get; }
    }
}
