namespace GameTheory.Games.FiveTribes.Moves
{
    public class PickUpMeeplesMove : Move
    {
        private Point point;

        public PickUpMeeplesMove(GameState state0, Point point)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var square = s1.Sultanate[point];

                return s1.With(
                    sultanate: s1.Sultanate.SetItem(point, square.With(meeples: EnumCollection<Meeple>.Empty)),
                    inHand: square.Meeples,
                    lastDirection: Direction.None,
                    lastPoint: point,
                    phase: Phase.MoveMeeples);
            })
        {
            this.point = point;
        }

        public Point Point
        {
            get { return this.point; }
        }

        public override string ToString()
        {
            return string.Format("Pick up meeples at {0}", this.point);
        }
    }
}
