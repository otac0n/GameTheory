// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move where a meeple is dropped on the board.
    /// </summary>
    public class DropMeepleMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropMeepleMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeple">The <see cref="Meeple"/> being dropped.</param>
        /// <param name="point">The <see cref="Point"/> at which the <see cref="Meeple"/> will be dropped.</param>
        public DropMeepleMove(GameState state, Meeple meeple, Point point)
            : base(state)
        {
            this.Meeple = meeple;
            this.Point = point;
        }

        /// <summary>
        /// Gets the <see cref="Meeple"/> being dropped.
        /// </summary>
        public Meeple Meeple { get; }

        /// <summary>
        /// Gets the <see cref="Point"/> at which the <see cref="Meeple"/> will be dropped.
        /// </summary>
        public Point Point { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => $"Drop {this.Meeple} at {this.Point}";

        internal override GameState Apply(GameState state)
        {
            var square = state.Sultanate[this.Point];
            var s1 = state.With(
                inHand: state.InHand.Remove(this.Meeple),
                lastPoint: this.Point,
                previousPoint: state.LastPoint,
                sultanate: state.Sultanate.SetItem(this.Point, square.With(meeples: square.Meeples.Add(this.Meeple))));

            return s1.InHand.Count >= 1 ? s1 : s1.WithMoves(s2 => new[]
            {
                new PickUpTribeMove(s2, this.Meeple),
            });
        }
    }
}
