// -----------------------------------------------------------------------
// <copyright file="PlacePalaceMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PlacePalaceMove : Move
    {
        private Func<GameState, GameState> after;
        private Point point;

        public PlacePalaceMove(GameState state0, Point point)
            : this(state0, point, s => s)
        {
        }

        public PlacePalaceMove(GameState state0, Point point, Func<GameState, GameState> after)
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
            return string.Format("Place a Palace at {0}", this.point);
        }

        internal override GameState Apply(GameState state0)
        {
            var square = state0.Sultanate[this.point];
            return this.after(state0.With(
                sultanate: state0.Sultanate.SetItem(this.point, square.With(palaces: square.Palaces + 1))));
        }

        internal Move With(GameState state, Point point)
        {
            return new PlacePalmTreeMove(state, point, this.after);
        }
    }
}
