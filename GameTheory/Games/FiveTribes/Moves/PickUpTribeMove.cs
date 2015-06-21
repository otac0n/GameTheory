// -----------------------------------------------------------------------
// <copyright file="PickUpTribeMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    public class PickUpTribeMove : Move
    {
        private readonly Meeple tribe;

        public PickUpTribeMove(GameState state0, Meeple tribe)
            : base(state0, state0.ActivePlayer)
        {
            this.tribe = tribe;
        }

        public override string ToString()
        {
            return string.Format("Pick up all {0} at {1}", this.tribe, this.State.LastPoint);
        }

        internal override GameState Apply(GameState state0)
        {
            var point = state0.LastPoint;
            var square = state0.Sultanate[point];
            var newSquare = square.With(meeples: square.Meeples.RemoveAll(this.tribe));
            var canAddCamel = newSquare.Owner == null && newSquare.Meeples.Count == 0 && state0.IsPlayerUnderCamelLimit(state0.ActivePlayer);

            return state0.With(
                sultanate: state0.Sultanate.SetItem(point, newSquare),
                inHand: state0.InHand.Add(this.tribe, square.Meeples[this.tribe]),
                phase: canAddCamel ? Phase.TileControlCheck : Phase.TribesAction);
        }
    }
}
