// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to bid for position on the <see cref="GameState.TurnOrderTrack"/>.
    /// </summary>
    public class BidMove : Move
    {
        private readonly int cost;
        private readonly int index;

        /// <summary>
        /// Initializes a new instance of the <see cref="BidMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The position on the <see cref="GameState.TurnOrderTrack"/> being bid on.</param>
        /// <param name="cost">The cost of the bid, in Gold Coins (GC).</param>
        public BidMove(GameState state, int index, int cost)
            : base(state, state.ActivePlayer)
        {
            this.index = index;
            this.cost = cost;
        }

        /// <summary>
        /// Gets the cost of the bid, in Gold Coins (GC).
        /// </summary>
        public int Cost
        {
            get { return this.cost; }
        }

        /// <summary>
        /// Gets the position on the <see cref="GameState.TurnOrderTrack"/> being bid on.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Bid {this.cost}";
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];
            var newQueue = state.BidOrderTrack.Dequeue();
            return state.With(
                bidOrderTrack: newQueue,
                inventory: state.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins - this.cost)),
                phase: newQueue.IsEmpty ? Phase.MoveTurnMarker : Phase.Bid,
                turnOrderTrack: state.TurnOrderTrack.SetItem(this.index, player));
        }
    }
}
