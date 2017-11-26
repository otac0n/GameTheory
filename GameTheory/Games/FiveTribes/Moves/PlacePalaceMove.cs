// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to place a Palace at a specified <see cref="Point"/>.
    /// </summary>
    public sealed class PlacePalaceMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlacePalaceMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The <see cref="Point"/> where a Palace will be placed.</param>
        /// <param name="phase">The <see cref="Phase"/> to transition to after the move has taken place.</param>
        public PlacePalaceMove(GameState state, Point point, Phase? phase = null)
            : base(state)
        {
            this.Point = point;
            this.Phase = phase;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Place a Palace at ", this.Point };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the <see cref="Phase"/> to transition to after the move has taken place.
        /// </summary>
        public Phase? Phase { get; }

        /// <summary>
        /// Gets the <see cref="Point"/> where a Palace will be placed.
        /// </summary>
        public Point Point { get; }

        internal override GameState Apply(GameState state)
        {
            var square = state.Sultanate[this.Point];
            state = state.With(
                sultanate: state.Sultanate.SetItem(this.Point, square.With(palaces: square.Palaces + 1)));

            if (this.Phase != null)
            {
                state = state.With(
                    phase: this.Phase.Value);
            }

            return state;
        }
    }
}
