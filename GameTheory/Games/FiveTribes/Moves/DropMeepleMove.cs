// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move where a meeple is dropped on the board.
    /// </summary>
    public sealed class DropMeepleMove : Move
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

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Drop ", this.Meeple, " at ", this.Point };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the <see cref="Meeple"/> being dropped.
        /// </summary>
        public Meeple Meeple { get; }

        /// <summary>
        /// Gets the <see cref="Point"/> at which the <see cref="Meeple"/> will be dropped.
        /// </summary>
        public Point Point { get; }

        internal static IEnumerable<DropMeepleMove> GenerateMoves(GameState state)
        {
            var drops = state.Sultanate.GetMoves(state.LastPoint, state.PreviousPoint, state.InHand);
            foreach (var drop in drops)
            {
                var meeple = drop.Item1;
                var point = drop.Item2;

                yield return new DropMeepleMove(state, meeple, point);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var square = state.Sultanate[this.Point];
            state = state.With(
                inHand: state.InHand.Remove(this.Meeple),
                lastPoint: this.Point,
                previousPoint: state.LastPoint,
                sultanate: state.Sultanate.SetItem(this.Point, square.With(meeples: square.Meeples.Add(this.Meeple))));

            return state.InHand.Count >= 1 ? state : state.WithInterstitialState(new DroppedLastMeeple(this.Meeple));
        }

        private class DroppedLastMeeple : InterstitialState
        {
            private Meeple meeple;

            public DroppedLastMeeple(Meeple meeple)
            {
                this.meeple = meeple;
            }

            public override int CompareTo(InterstitialState other)
            {
                if (other is DroppedLastMeeple d)
                {
                    return this.meeple.CompareTo(d.meeple);
                }
                else
                {
                    return base.CompareTo(other);
                }
            }

            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                yield return new PickUpTribeMove(state, this.meeple);
            }
        }
    }
}
