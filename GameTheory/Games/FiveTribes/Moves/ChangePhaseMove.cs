// -----------------------------------------------------------------------
// <copyright file="ChangePhaseMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    public class ChangePhaseMove : Move
    {
        private string description;
        private Phase phase;

        public ChangePhaseMove(GameState state0, string description, Phase phase)
            : base(state0, state0.ActivePlayer)
        {
            this.description = description;
            this.phase = phase;
        }

        public Phase Phase
        {
            get { return this.phase; }
        }

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
