// -----------------------------------------------------------------------
// <copyright file="PickUpMeeplesMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    public class PickUpMeeplesMove : Move
    {
        private Point point;

        public PickUpMeeplesMove(GameState state0, Point point)
            : base(state0, state0.ActivePlayer)
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

        internal override GameState Apply(GameState state0)
        {
            var square = state0.Sultanate[this.point];

            return state0.With(
                sultanate: state0.Sultanate.SetItem(this.point, square.With(meeples: EnumCollection<Meeple>.Empty)),
                inHand: square.Meeples,
                lastDirection: Direction.None,
                lastPoint: this.point,
                phase: Phase.MoveMeeples);
        }
    }
}
