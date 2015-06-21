namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Immutable;
    using System.Linq;

    public class EndTurnMove : Move
    {
        public EndTurnMove(GameState state0)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                ImmutableList<Djinn> dealtDjinns;
                var djinnDiscards = s1.DjinnDiscards;
                var djinnPile = s1.DjinnPile.Deal(3 - s1.VisibleDjinns.Count, out dealtDjinns, ref djinnDiscards);

                ImmutableList<Resource> dealtResources;
                var resourceDiscards = s1.ResourceDiscards;
                var resourcePile = s1.ResourcePile.Deal(9 - s1.VisibleResources.Count, out dealtResources, ref resourceDiscards);

                var moreTurns = s1.GetHighestBidIndex() != -1;
                var isOver = !moreTurns && (s1.Players.Any(p => !s1.IsPlayerUnderCamelLimit(p)) || !s1.Sultanate.GetPickups().Any());

                return s1.With(
                    djinnDiscards: djinnDiscards,
                    djinnPile: djinnPile,
                    lastDirection: Direction.None,
                    lastPoint: new Point(),
                    phase: moreTurns ? Phase.MoveTurnMarker : (isOver ? Phase.End : Phase.Bid),
                    resourceDiscards: resourceDiscards,
                    resourcePile: resourcePile,
                    visibleDjinns: s1.VisibleDjinns.AddRange(dealtDjinns),
                    visibleResources: s1.VisibleResources.AddRange(dealtResources));
            })
        {
        }

        public override string ToString()
        {
            return "End turn";
        }
    }
}
