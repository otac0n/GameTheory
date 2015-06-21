// -----------------------------------------------------------------------
// <copyright file="BidMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

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
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The position on the <see cref="GameState.TurnOrderTrack"/> being bid on.</param>
        /// <param name="cost">The cost of the bid, in Gold Coins (GC).</param>
        public BidMove(GameState state0, int index, int cost)
            : base(state0, state0.ActivePlayer)
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
            return string.Format("Bid {0}", this.cost);
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];
            var newQueue = state0.BidOrderTrack.Dequeue();
            return state0.With(
                bidOrderTrack: newQueue,
                turnOrderTrack: state0.TurnOrderTrack.SetItem(this.index, player),
                inventory: state0.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins - this.cost)),
                phase: newQueue.IsEmpty ? Phase.MoveTurnMarker : Phase.Bid);
        }
    }
}
