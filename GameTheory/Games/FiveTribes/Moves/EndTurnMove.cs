// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to end the active player's turn.
    /// </summary>
    public sealed class EndTurnMove : Move
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
        public override bool IsDeterministic => this.State.FindHighestBidIndex() != -1;

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "End turn" };

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            yield return new EndTurnMove(state);
        }

        internal override GameState Apply(GameState state)
        {
            state = state.With(
                lastPoint: default(Point),
                previousPoint: default(Point));

            if (state.FindHighestBidIndex() != -1)
            {
                return state.With(
                    phase: Phase.MoveTurnMarker);
            }
            else
            {
                var isOver = state.Players.Any(p => !state.IsPlayerUnderCamelLimit(p)) || !state.Sultanate.GetPickUps().Any();

                var djinnDiscards = state.DjinnDiscards;
                var djinnPile = state.DjinnPile.Deal(3 - state.VisibleDjinns.Count, out ImmutableList<Djinn> dealtDjinns, ref djinnDiscards);

                var resourceDiscards = state.ResourceDiscards;
                var resourcePile = state.ResourcePile.Deal(9 - state.VisibleResources.Count, out ImmutableList<Resource> dealtResources, ref resourceDiscards);

                return state.With(
                    djinnDiscards: djinnDiscards,
                    djinnPile: djinnPile,
                    phase: isOver ? Phase.End : Phase.Bid,
                    resourceDiscards: resourceDiscards,
                    resourcePile: resourcePile,
                    visibleDjinns: state.VisibleDjinns.AddRange(dealtDjinns),
                    visibleResources: state.VisibleResources.AddRange(dealtResources));
            }
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            // TODO: Implement outcomes.
            return base.GetOutcomes(state);
        }
    }
}
