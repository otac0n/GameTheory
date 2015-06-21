namespace GameTheory.Games.FiveTribes.Moves
{
    public class DropMeepleMove : Move
    {
        private readonly Meeple meeple;
        private readonly Point point;

        public DropMeepleMove(GameState state0, Meeple meeple, Point point)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var square1 = s1.Sultanate[point];
                var s2 = s1.With(
                    sultanate: s1.Sultanate.SetItem(point, square1.With(meeples: square1.Meeples.Add(meeple))),
                    inHand: s1.InHand.Remove(meeple),
                    lastDirection: Sultanate.GetDirection(from: s1.LastPoint, to: point),
                    lastPoint: point);

                return s2.InHand.Count >= 1 ? s2 : s2.WithMoves(s3 => new[]
                {
                    new PickUpTribeMove(s3, meeple),
                });
            })
        {
            this.meeple = meeple;
            this.point = point;
        }

        public Meeple Meeple
        {
            get { return this.meeple; }
        }

        public Point Point
        {
            get { return this.point; }
        }

        public override string ToString()
        {
            return string.Format("Drop {0} at {1}", this.meeple, this.point);
        }
    }
}
