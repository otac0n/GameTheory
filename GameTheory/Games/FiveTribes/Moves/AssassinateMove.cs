// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Linq;

    /// <summary>
    /// Represents a move to assassinate specific <see cref="Meeple">Meeples</see> at a specific <see cref="Point"/> on the board.
    /// </summary>
    public class AssassinateMove : Move
    {
        private readonly Func<GameState, GameState> after;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinateMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The <see cref="Point"/> at which the assassination will take place.</param>
        /// <param name="meeples">The <see cref="Meeple">Meeples</see> that will be assassinated.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public AssassinateMove(GameState state, Point point, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state)
        {
            this.after = after;
            this.Meeples = meeples;
            this.Point = point;
        }

        /// <summary>
        /// Gets the <see cref="Meeple">Meeples</see> that will be assassinated.
        /// </summary>
        public EnumCollection<Meeple> Meeples { get; }

        /// <summary>
        /// Gets the <see cref="Point"/> at which the assassination will take place.
        /// </summary>
        public Point Point { get; }

        /// <inheritdoc />
        public override bool IsDeterministic =>
            this.Meeples[Meeple.Merchant] == 0 || !this.State.Inventory[this.State.ActivePlayer].Djinns.Any(d => d is Djinns.Kandicha);

        /// <inheritdoc />
        public override string ToString() => $"Assissinate {this.Meeples} at {this.Point}";

        internal override GameState Apply(GameState state)
        {
            var square = state.Sultanate[this.Point];
            var newSquare = square.With(meeples: square.Meeples.RemoveRange(this.Meeples));
            var newState = state.With(
                bag: state.Bag.AddRange(state.InHand).AddRange(this.Meeples),
                inHand: EnumCollection<Meeple>.Empty,
                sultanate: state.Sultanate.SetItem(this.Point, newSquare));

            foreach (var owner in newState.Players)
            {
                foreach (var djinn in newState.Inventory[owner].Djinns)
                {
                    newState = djinn.HandleAssassination(owner, newState, this.Point, this.Meeples);
                }
            }

            if (newSquare.Meeples.Count == 0 && newSquare.Owner == null && newState.IsPlayerUnderCamelLimit(state.ActivePlayer))
            {
                return newState.WithMoves(t => new PlaceCamelMove(t, this.Point, this.after));
            }
            else
            {
                return this.after(newState);
            }
        }
    }
}
