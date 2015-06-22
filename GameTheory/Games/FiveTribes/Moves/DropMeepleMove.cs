// -----------------------------------------------------------------------
// <copyright file="DropMeepleMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move where a meeple is dropped on the board.
    /// </summary>
    public class DropMeepleMove : Move
    {
        private readonly Meeple meeple;
        private readonly Point point;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropMeepleMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeple">The <see cref="Meeple"/> being dropped.</param>
        /// <param name="point">The <see cref="Point"/> at which the <see cref="Meeple"/> will be dropped.</param>
        public DropMeepleMove(GameState state0, Meeple meeple, Point point)
            : base(state0, state0.ActivePlayer)
        {
            this.meeple = meeple;
            this.point = point;
        }

        /// <summary>
        /// Gets the <see cref="Meeple"/> being dropped.
        /// </summary>
        public Meeple Meeple
        {
            get { return this.meeple; }
        }

        /// <summary>
        /// Gets the <see cref="Point"/> at which the <see cref="Meeple"/> will be dropped.
        /// </summary>
        public Point Point
        {
            get { return this.point; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Drop {0} at {1}", this.meeple, this.point);
        }

        internal override GameState Apply(GameState state0)
        {
            var square = state0.Sultanate[this.point];
            var s1 = state0.With(
                inHand: state0.InHand.Remove(this.meeple),
                lastPoint: this.point,
                previousPoint: state0.LastPoint,
                sultanate: state0.Sultanate.SetItem(this.point, square.With(meeples: square.Meeples.Add(this.meeple))));

            return s1.InHand.Count >= 1 ? s1 : s1.WithMoves(s2 => new[]
            {
                new PickUpTribeMove(s2, this.meeple),
            });
        }
    }
}
