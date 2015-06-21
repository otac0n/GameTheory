namespace GameTheory.Games.FiveTribes.Moves
{
    public class PickUpTribeMove : Move
    {
        private readonly Meeple tribe;

        public PickUpTribeMove(GameState state0, Meeple tribe)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var point = s1.LastPoint;
                var square4 = s1.Sultanate[point];
                var newSquare = square4.With(meeples: square4.Meeples.RemoveAll(tribe));
                var canAddCamel = newSquare.Owner == null && newSquare.Meeples.Count == 0 && s1.IsPlayerUnderCamelLimit(s1.ActivePlayer);

                return s1.With(
                    sultanate: s1.Sultanate.SetItem(point, newSquare),
                    inHand: s1.InHand.Add(tribe, square4.Meeples[tribe]),
                    phase: canAddCamel ? Phase.TileControlCheck : Phase.TribesAction);
            })
        {
            this.tribe = tribe;
        }

        public override string ToString()
        {
            return string.Format("Pick up all {0} at {1}", tribe, this.State.LastPoint);
        }
    }
}
