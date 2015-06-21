// -----------------------------------------------------------------------
// <copyright file="AssassinateMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class AssassinateMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Meeple> meeples;
        private readonly Point point;

        public AssassinateMove(GameState state0, Point point, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.meeples = meeples;
            this.point = point;
        }

        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        public Point Point
        {
            get { return this.point; }
        }

        public override string ToString()
        {
            return string.Format("Assissinate {0} at {1}", string.Join(",", this.meeples), this.point);
        }

        internal override GameState Apply(GameState state0)
        {
            var square = state0.Sultanate[this.point];
            var newSquare = square.With(meeples: square.Meeples.RemoveRange(this.meeples));
            var newState = state0.With(
                bag: state0.Bag.AddRange(state0.InHand).AddRange(this.meeples),
                sultanate: state0.Sultanate.SetItem(this.point, newSquare),
                inHand: EnumCollection<Meeple>.Empty);

            foreach (var owner in newState.Players)
            {
                foreach (var djinn in newState.Inventory[owner].Djinns)
                {
                    newState = djinn.HandleAssassination(owner, newState, this.point, this.meeples);
                }
            }

            if (newSquare.Meeples.Count == 0 && newSquare.Owner == null && newState.IsPlayerUnderCamelLimit(state0.ActivePlayer))
            {
                return newState.WithMoves(t => new PlaceCamelMove(t, this.point, this.after));
            }
            else
            {
                return this.after(newState);
            }
        }
    }
}
