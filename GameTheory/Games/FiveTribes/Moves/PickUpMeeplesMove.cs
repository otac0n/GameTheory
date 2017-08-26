﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to pick up all of the <see cref="Meeple">Meeples</see> at the specified <see cref="Point"/>.
    /// </summary>
    public class PickUpMeeplesMove : Move
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

        /// <summary>
        /// Gets the <see cref="Point"/> in the <see cref="GameState.Sultanate"/> where the <see cref="Meeple">Meeples</see> will be picked up.
        /// </summary>
        public Point Point { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => $"Pick up meeples at {this.Point}";

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
