// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move that skips to a specified <see cref="Phase"/>.
    /// </summary>
    public class ChangePhaseMove : Move
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

        /// <summary>
        /// Gets the phase that the move will change to.
        /// </summary>
        public Phase Phase { get; }

        /// <inheritdoc />
        public override string ToString() => this.description;

        internal override GameState Apply(GameState state)
        {
            return state.With(phase: this.Phase);
        }
    }
}
