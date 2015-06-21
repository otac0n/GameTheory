// -----------------------------------------------------------------------
// <copyright file="PlacePalmTreeMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PlacePalmTreeMove : Move
    {
        private Func<GameState, GameState> after;
        private Point point;

        public PlacePalmTreeMove(GameState state0, Point point)
            : this(state0, point, s => s)
        {
        }

        public PlacePalmTreeMove(GameState state0, Point point, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.point = point;
            this.after = after;
        }

        public Point Point
        {
            get { return this.point; }
        }

        public override string ToString()
        {
            return string.Format("Place a Palm Tree at {0}", this.point);
        }

        internal override GameState Apply(GameState state0)
        {
            var square = state0.Sultanate[this.point];
            return this.after(state0.With(
                sultanate: state0.Sultanate.SetItem(this.point, square.With(palmTrees: square.PalmTrees + 1))));
        }

        internal Move With(GameState state, Point point)
        {
            return new PlacePalmTreeMove(state, point, this.after);
        }
    }
}
