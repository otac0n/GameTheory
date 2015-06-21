// -----------------------------------------------------------------------
// <copyright file="ChangePhaseMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move that skips to a specified <see cref="Phase"/>.
    /// </summary>
    public class ChangePhaseMove : Move
    {
        private string description;
        private Phase phase;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePhaseMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="description">The description of the move.</param>
        /// <param name="phase">The <see cref="Phase"/> that the move will change to.</param>
        public ChangePhaseMove(GameState state0, string description, Phase phase)
            : base(state0, state0.ActivePlayer)
        {
            this.description = description;
            this.phase = phase;
        }

        /// <summary>
        /// Gets the phase that the move will change to.
        /// </summary>
        public Phase Phase
        {
            get { return this.phase; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.description;
        }

        internal override GameState Apply(GameState state0)
        {
            return state0.With(phase: this.phase);
        }
    }
}
