// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public MoveTurnMarkerMove(GameState state)
            : base(state, state.ActivePlayer)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Move turn marker";
        }

        internal override GameState Apply(GameState state)
        {
            var i = state.FindHighestBidIndex();
            return state.With(
                bidOrderTrack: state.BidOrderTrack.Enqueue(state.TurnOrderTrack[i]),
                phase: Phase.PickUpMeeples,
                turnOrderTrack: state.TurnOrderTrack.SetItem(i, null));
        }
    }
}
