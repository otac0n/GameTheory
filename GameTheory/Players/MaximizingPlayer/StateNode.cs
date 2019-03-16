// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System.Collections.Generic;
    using System.Linq;

    public class StateNode<TMove, TScore>
        where TMove : IMove
    {
        private readonly Dictionary<TMove, MoveNode<TMove, TScore>> results = new Dictionary<TMove, MoveNode<TMove, TScore>>();
        private readonly GameTree<TMove, TScore> tree;
        private TMove[] moves;

        public StateNode(GameTree<TMove, TScore> tree, IGameState<TMove> state)
        {
            this.tree = tree;
            this.State = state;
        }

        public Mainline<TMove, TScore> Mainline { get; set; }

        public TMove[] Moves => this.moves ?? (this.moves = this.State.GetAvailableMoves().ToArray());

        public IDictionary<PlayerToken, TScore> Score { get; set; }

        public IGameState<TMove> State { get; }

        public MoveNode<TMove, TScore> this[TMove move]
        {
            get
            {
                if (!this.results.TryGetValue(move, out var result))
                {
                    this.results[move] = result = new MoveNode<TMove, TScore>(this.tree, this.State.GetOutcomes(move));
                }

                return result;
            }
        }
    }
}
