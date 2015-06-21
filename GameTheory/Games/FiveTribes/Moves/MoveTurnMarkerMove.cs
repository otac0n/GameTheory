namespace GameTheory.Games.FiveTribes.Moves
{
    public class MoveTurnMarkerMove : Move
    {
        public MoveTurnMarkerMove(GameState state0)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var i = s1.GetHighestBidIndex();
                return s1.With(
                    bidOrderTrack: s1.BidOrderTrack.Enqueue(s1.TurnOrderTrack[i]),
                    turnOrderTrack: s1.TurnOrderTrack.SetItem(i, null),
                    phase: Phase.PickUpMeeples);
            })
        {
        }

        public override string ToString()
        {
            return "Move turn marker";
        }
    }
}
