// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to pick up all <see cref="Meeple">Meeples</see> of a specific tribe.
    /// </summary>
    public sealed class PickUpTribeMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PickUpTribeMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="tribe">The <see cref="Meeple"/> tribe to pick up.</param>
        public PickUpTribeMove(GameState state, Meeple tribe)
            : base(state)
        {
            this.Tribe = tribe;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Pick up all ", this.Tribe, " at ", this.State.LastPoint };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the <see cref="Meeple"/> tribe to pick up.
        /// </summary>
        public Meeple Tribe { get; }

        internal override GameState Apply(GameState state)
        {
            var point = state.LastPoint;
            var square = state.Sultanate[point];
            var newSquare = square.With(meeples: square.Meeples.RemoveAll(this.Tribe));
            var canAddCamel = newSquare.Owner == null && newSquare.Meeples.Count == 0 && state.IsPlayerUnderCamelLimit(state.ActivePlayer);

            return state.With(
                inHand: state.InHand.Add(this.Tribe, square.Meeples[this.Tribe]),
                phase: canAddCamel ? Phase.TileControlCheck : Phase.TribesAction,
                sultanate: state.Sultanate.SetItem(point, newSquare));
        }
    }
}
