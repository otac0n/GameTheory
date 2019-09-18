// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to move the active player's turn marker.
    /// </summary>
    public sealed class MoveTurnMarkerMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoveTurnMarkerMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public MoveTurnMarkerMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { Resources.MoveTurnMarker };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        internal static IEnumerable<MoveTurnMarkerMove> GenerateMoves(GameState state)
        {
            yield return new MoveTurnMarkerMove(state);
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
