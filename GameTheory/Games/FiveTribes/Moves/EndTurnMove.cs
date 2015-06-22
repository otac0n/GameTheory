// -----------------------------------------------------------------------
// <copyright file="EndTurnMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to end the active player's turn.
    /// </summary>
    public class EndTurnMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndTurnMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        public EndTurnMove(GameState state0)
            : base(state0, state0.ActivePlayer)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "End turn";
        }

        internal override GameState Apply(GameState state0)
        {
            ImmutableList<Djinn> dealtDjinns;
            var djinnDiscards = state0.DjinnDiscards;
            var djinnPile = state0.DjinnPile.Deal(3 - state0.VisibleDjinns.Count, out dealtDjinns, ref djinnDiscards);

            ImmutableList<Resource> dealtResources;
            var resourceDiscards = state0.ResourceDiscards;
            var resourcePile = state0.ResourcePile.Deal(9 - state0.VisibleResources.Count, out dealtResources, ref resourceDiscards);

            var moreTurns = state0.GetHighestBidIndex() != -1;
            var isOver = !moreTurns && (state0.Players.Any(p => !state0.IsPlayerUnderCamelLimit(p)) || !state0.Sultanate.GetPickUps().Any());

            return state0.With(
                djinnDiscards: djinnDiscards,
                djinnPile: djinnPile,
                lastPoint: new Point(),
                phase: moreTurns ? Phase.MoveTurnMarker : (isOver ? Phase.End : Phase.Bid),
                previousPoint: new Point(),
                resourceDiscards: resourceDiscards,
                resourcePile: resourcePile,
                visibleDjinns: state0.VisibleDjinns.AddRange(dealtDjinns),
                visibleResources: state0.VisibleResources.AddRange(dealtResources));
        }
    }
}
