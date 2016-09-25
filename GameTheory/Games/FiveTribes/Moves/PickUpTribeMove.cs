// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to pick up all <see cref="Meeple">Meeples</see> of a specific tribe.
    /// </summary>
    public class PickUpTribeMove : Move
    {
        private readonly Meeple tribe;

        /// <summary>
        /// Initializes a new instance of the <see cref="PickUpTribeMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="tribe">The <see cref="Meeple"/> tribe to pick up.</param>
        public PickUpTribeMove(GameState state, Meeple tribe)
            : base(state, state.ActivePlayer)
        {
            this.tribe = tribe;
        }

        /// <summary>
        /// Gets the <see cref="Meeple"/> tribe to pick up.
        /// </summary>
        public Meeple Tribe
        {
            get { return this.tribe; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Pick up all {this.tribe} at {this.State.LastPoint}";
        }

        internal override GameState Apply(GameState state)
        {
            var point = state.LastPoint;
            var square = state.Sultanate[point];
            var newSquare = square.With(meeples: square.Meeples.RemoveAll(this.tribe));
            var canAddCamel = newSquare.Owner == null && newSquare.Meeples.Count == 0 && state.IsPlayerUnderCamelLimit(state.ActivePlayer);

            return state.With(
                inHand: state.InHand.Add(this.tribe, square.Meeples[this.tribe]),
                phase: canAddCamel ? Phase.TileControlCheck : Phase.TribesAction,
                sultanate: state.Sultanate.SetItem(point, newSquare));
        }
    }
}
