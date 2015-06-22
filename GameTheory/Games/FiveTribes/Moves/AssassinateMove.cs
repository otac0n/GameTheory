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

    /// <summary>
    /// Represents a move to assassinate specific <see cref="Meeple">Meeples</see> at a specific <see cref="Point"/> on the board.
    /// </summary>
    public class AssassinateMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Meeple> meeples;
        private readonly Point point;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinateMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The <see cref="Point"/> at which the assassination will take place.</param>
        /// <param name="meeples">The <see cref="Meeple">Meeples</see> that will be assassinated.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public AssassinateMove(GameState state0, Point point, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.meeples = meeples;
            this.point = point;
        }

        /// <summary>
        /// Gets the <see cref="Meeple">Meeples</see> that will be assassinated.
        /// </summary>
        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        /// <summary>
        /// Gets the <see cref="Point"/> at which the assassination will take place.
        /// </summary>
        public Point Point
        {
            get { return this.point; }
        }

        /// <inheritdoc />
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
                inHand: EnumCollection<Meeple>.Empty,
                sultanate: state0.Sultanate.SetItem(this.point, newSquare));

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
