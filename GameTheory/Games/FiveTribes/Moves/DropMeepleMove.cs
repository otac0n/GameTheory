// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

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
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeple">The <see cref="Meeple"/> being dropped.</param>
        /// <param name="point">The <see cref="Point"/> at which the <see cref="Meeple"/> will be dropped.</param>
        public DropMeepleMove(GameState state, Meeple meeple, Point point)
            : base(state, state.ActivePlayer)
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
            return $"Drop {this.meeple} at {this.point}";
        }

        internal override GameState Apply(GameState state)
        {
            var square = state.Sultanate[this.point];
            var s1 = state.With(
                inHand: state.InHand.Remove(this.meeple),
                lastPoint: this.point,
                previousPoint: state.LastPoint,
                sultanate: state.Sultanate.SetItem(this.point, square.With(meeples: square.Meeples.Add(this.meeple))));

            return s1.InHand.Count >= 1 ? s1 : s1.WithMoves(s2 => new[]
            {
                new PickUpTribeMove(s2, this.meeple),
            });
        }
    }
}
