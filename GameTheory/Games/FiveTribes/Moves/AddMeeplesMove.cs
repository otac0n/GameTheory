// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        private readonly Point point;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMeeplesMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The <see cref="Point"/> where the <see cref="Meeple">Meeples</see> will be added.</param>
        public AddMeeplesMove(GameState state, Point point)
            : base(state, state.ActivePlayer)
        {
            this.point = point;
        }

        /// <summary>
        /// Gets the <see cref="Point"/> where the <see cref="Meeple">Meeples</see> will be added.
        /// </summary>
        public Point Point
        {
            get { return this.point; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Draw {Math.Min(this.State.Bag.Count, 3)} Meeples and place at {this.point}";
        }

        internal override GameState Apply(GameState state)
        {
            ImmutableList<Meeple> dealt;
            var newBag = state.Bag;
            newBag = newBag.Deal(3, out dealt);

            return state.With(
                bag: newBag,
                sultanate: state.Sultanate.SetItem(this.point, state.Sultanate[this.point].With(meeples: new EnumCollection<Meeple>(dealt))));
        }
    }
}
