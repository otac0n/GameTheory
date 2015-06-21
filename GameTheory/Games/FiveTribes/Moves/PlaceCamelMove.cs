// -----------------------------------------------------------------------
// <copyright file="PlaceCamelMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PlaceCamelMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly Point point;

        public PlaceCamelMove(GameState state0, Point point)
            : this(state0, point, s => s)
        {
        }

        public PlaceCamelMove(GameState state0, Point point, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.point = point;
        }

        public Point Point
        {
            get { return this.point; }
        }

        public override string ToString()
        {
            return string.Format("Place a Camel at {0}", this.point);
        }

        internal override GameState Apply(GameState state0)
        {
            var owner = state0.ActivePlayer;

            return this.after(state0.With(
                sultanate: state0.Sultanate.SetItem(this.point, state0.Sultanate[this.point].With(owner: owner))));
        }
    }
}
