// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to bid for position on the <see cref="GameState.TurnOrderTrack"/>.
    /// </summary>
    public sealed class BidMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BidMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The position on the <see cref="GameState.TurnOrderTrack"/> being bid on.</param>
        /// <param name="cost">The cost of the bid, in Gold Coins (GC).</param>
        public BidMove(GameState state, int index, int cost)
            : base(state)
        {
            this.Index = index;
            this.Cost = cost;
        }

        /// <summary>
        /// Gets the cost of the bid, in Gold Coins (GC).
        /// </summary>
        public int Cost { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.Bid, this.Cost);

        /// <summary>
        /// Gets the position on the <see cref="GameState.TurnOrderTrack"/> being bid on.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        internal static IEnumerable<BidMove> GenerateMoves(GameState state)
        {
            for (var i = 2; i < state.TurnOrderTrack.Count; i++)
            {
                if (state.TurnOrderTrack[i] == null && state.Inventory[state.ActivePlayer].GoldCoins >= GameState.TurnOrderTrackCosts[i])
                {
                    var j = i == 2 && state.TurnOrderTrack[0] == null ? 0 :
                            i == 2 && state.TurnOrderTrack[1] == null ? 1 :
                            i;

                    yield return new BidMove(state, j, GameState.TurnOrderTrackCosts[j]);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];
            var newQueue = state.BidOrderTrack.Dequeue();
            return state.With(
                bidOrderTrack: newQueue,
                inventory: state.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins - this.Cost)),
                phase: newQueue.IsEmpty ? Phase.MoveTurnMarker : Phase.Bid,
                turnOrderTrack: state.TurnOrderTrack.SetItem(this.Index, player));
        }
    }
}
