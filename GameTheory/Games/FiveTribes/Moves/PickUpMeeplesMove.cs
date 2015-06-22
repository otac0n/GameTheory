// -----------------------------------------------------------------------
// <copyright file="PickUpMeeplesMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to pick up all of the <see cref="Meeple">Meeples</see> at the specified <see cref="Point"/>.
    /// </summary>
    public class PickUpMeeplesMove : Move
    {
        private Point point;

        /// <summary>
        /// Initializes a new instance of the <see cref="PickUpMeeplesMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The <see cref="Point"/> in the <see cref="GameState.Sultanate"/> where the <see cref="Meeple">Meeples</see> will be picked up.</param>
        public PickUpMeeplesMove(GameState state0, Point point)
            : base(state0, state0.ActivePlayer)
        {
            this.point = point;
        }

        /// <summary>
        /// Gets the <see cref="Point"/> in the <see cref="GameState.Sultanate"/> where the <see cref="Meeple">Meeples</see> will be picked up.
        /// </summary>
        public Point Point
        {
            get { return this.point; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Pick up meeples at {0}", this.point);
        }

        internal override GameState Apply(GameState state0)
        {
            var square = state0.Sultanate[this.point];

            return state0.With(
                inHand: square.Meeples,
                lastPoint: this.point,
                phase: Phase.MoveMeeples,
                previousPoint: this.point,
                sultanate: state0.Sultanate.SetItem(this.point, square.With(meeples: EnumCollection<Meeple>.Empty)));
        }
    }
}
