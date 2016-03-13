// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to move the active player's turn marker.
    /// </summary>
    public class MoveTurnMarkerMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoveTurnMarkerMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        public MoveTurnMarkerMove(GameState state0)
            : base(state0, state0.ActivePlayer)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Move turn marker";
        }

        internal override GameState Apply(GameState state0)
        {
            var i = state0.GetHighestBidIndex();
            return state0.With(
                bidOrderTrack: state0.BidOrderTrack.Enqueue(state0.TurnOrderTrack[i]),
                phase: Phase.PickUpMeeples,
                turnOrderTrack: state0.TurnOrderTrack.SetItem(i, null));
        }
    }
}
