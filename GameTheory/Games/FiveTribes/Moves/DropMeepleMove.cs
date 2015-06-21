// -----------------------------------------------------------------------
// <copyright file="DropMeepleMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    public class DropMeepleMove : Move
    {
        private readonly Meeple meeple;
        private readonly Point point;

        public DropMeepleMove(GameState state0, Meeple meeple, Point point)
            : base(state0, state0.ActivePlayer)
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

        internal override GameState Apply(GameState state0)
        {
            var square = state0.Sultanate[this.point];
            var s1 = state0.With(
                sultanate: state0.Sultanate.SetItem(this.point, square.With(meeples: square.Meeples.Add(this.meeple))),
                inHand: state0.InHand.Remove(this.meeple),
                lastDirection: Sultanate.GetDirection(from: state0.LastPoint, to: this.point),
                lastPoint: this.point);

            return s1.InHand.Count >= 1 ? s1 : s1.WithMoves(s2 => new[]
            {
                new PickUpTribeMove(s2, this.meeple),
            });
        }
    }
}
