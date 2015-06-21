namespace GameTheory.Games.FiveTribes.Moves
{
    public class ChangePhaseMove : Move
    {
        private string description;
        private Phase phase;

        public ChangePhaseMove(GameState state0, string description, Phase phase)
            : base(state0, state0.ActivePlayer, s1 => s1.With(phase: phase))
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
    }
}
