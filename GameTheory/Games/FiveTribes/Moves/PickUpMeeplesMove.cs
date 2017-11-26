// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to pick up all of the <see cref="Meeple">Meeples</see> at the specified <see cref="Point"/>.
    /// </summary>
    public sealed class PickUpMeeplesMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PickUpMeeplesMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The <see cref="Point"/> in the <see cref="GameState.Sultanate"/> where the <see cref="Meeple">Meeples</see> will be picked up.</param>
        public PickUpMeeplesMove(GameState state, Point point)
            : base(state)
        {
            this.Point = point;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Pick up meeples at ", this.Point };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the <see cref="Point"/> in the <see cref="GameState.Sultanate"/> where the <see cref="Meeple">Meeples</see> will be picked up.
        /// </summary>
        public Point Point { get; }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var any = false;
            foreach (var point in state.Sultanate.GetPickUps())
            {
                any = true;
                yield return new PickUpMeeplesMove(state, point);
            }

            if (!any)
            {
                yield return new ChangePhaseMove(state, "Skip move", Phase.MerchandiseSale);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var square = state.Sultanate[this.Point];

            return state.With(
                inHand: square.Meeples,
                lastPoint: this.Point,
                phase: Phase.MoveMeeples,
                previousPoint: this.Point,
                sultanate: state.Sultanate.SetItem(this.Point, square.With(meeples: EnumCollection<Meeple>.Empty)));
        }
    }
}
