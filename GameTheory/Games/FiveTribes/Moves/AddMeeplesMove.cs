﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Collections.Immutable;
    using GameTheory.Games.FiveTribes.Tiles;

    /// <summary>
    /// Represents a move to draw three <see cref="Meeple">Meeples</see> and place them on the specified <see cref="Tile"/>.
    /// </summary>
    public class AddMeeplesMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddMeeplesMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The <see cref="Point"/> where the <see cref="Meeple">Meeples</see> will be added.</param>
        public AddMeeplesMove(GameState state, Point point)
            : base(state)
        {
            this.Point = point;
        }

        /// <summary>
        /// Gets the <see cref="Point"/> where the <see cref="Meeple">Meeples</see> will be added.
        /// </summary>
        public Point Point { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => false;

        /// <inheritdoc />
        public override string ToString() => $"Draw {Math.Min(this.State.Bag.Count, 3)} Meeples and place at {this.Point}";

        internal override GameState Apply(GameState state)
        {
            var newBag = state.Bag.Deal(3, out ImmutableList<Meeple> dealt);

            return state.With(
                bag: newBag,
                sultanate: state.Sultanate.SetItem(this.Point, state.Sultanate[this.Point].With(meeples: new EnumCollection<Meeple>(dealt))));
        }
    }
}
