// -----------------------------------------------------------------------
// <copyright file="MoveTurnMarkerMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    public class MoveTurnMarkerMove : Move
    {
        public MoveTurnMarkerMove(GameState state0)
            : base(state0, state0.ActivePlayer)
        {
        }

        public override string ToString()
        {
            return "Move turn marker";
        }

        internal override GameState Apply(GameState state0)
        {
            var i = state0.GetHighestBidIndex();
            return state0.With(
                bidOrderTrack: state0.BidOrderTrack.Enqueue(state0.TurnOrderTrack[i]),
                turnOrderTrack: state0.TurnOrderTrack.SetItem(i, null),
                phase: Phase.PickUpMeeples);
        }
    }
}
