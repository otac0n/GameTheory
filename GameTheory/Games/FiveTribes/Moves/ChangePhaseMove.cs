// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move that skips to a specified <see cref="Phase"/>.
    /// </summary>
    public sealed class ChangePhaseMove : Move
    {
        private string description;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePhaseMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="description">The description of the move.</param>
        /// <param name="phase">The <see cref="Phase"/> that the move will change to.</param>
        public ChangePhaseMove(GameState state, string description, Phase phase)
            : base(state)
        {
            this.description = description;
            this.Phase = phase;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { this.description };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the phase that the move will change to.
        /// </summary>
        public Phase Phase { get; }

        internal override GameState Apply(GameState state)
        {
            return state.With(phase: this.Phase);
        }
    }
}
