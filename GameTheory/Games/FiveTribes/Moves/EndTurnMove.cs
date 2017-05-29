// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Djinns;

    /// <summary>
    /// Represents a move to end the active player's turn.
    /// </summary>
    public class EndTurnMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndTurnMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public EndTurnMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override string ToString() => "End turn";

        internal override GameState Apply(GameState state)
        {
            var djinnDiscards = state.DjinnDiscards;
            var djinnPile = state.DjinnPile.Deal(3 - state.VisibleDjinns.Count, out ImmutableList<Djinn> dealtDjinns, ref djinnDiscards);

            var resourceDiscards = state.ResourceDiscards;
            var resourcePile = state.ResourcePile.Deal(9 - state.VisibleResources.Count, out ImmutableList<Resource> dealtResources, ref resourceDiscards);

            var moreTurns = state.FindHighestBidIndex() != -1;
            var isOver = !moreTurns && (state.Players.Any(p => !state.IsPlayerUnderCamelLimit(p)) || !state.Sultanate.GetPickUps().Any());

            return state.With(
                djinnDiscards: djinnDiscards,
                djinnPile: djinnPile,
                lastPoint: default(Point),
                phase: moreTurns ? Phase.MoveTurnMarker : (isOver ? Phase.End : Phase.Bid),
                previousPoint: default(Point),
                resourceDiscards: resourceDiscards,
                resourcePile: resourcePile,
                visibleDjinns: state.VisibleDjinns.AddRange(dealtDjinns),
                visibleResources: state.VisibleResources.AddRange(dealtResources));
        }
    }
}
