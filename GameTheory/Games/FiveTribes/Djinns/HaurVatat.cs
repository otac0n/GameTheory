namespace GameTheory.Games.FiveTribes.Djinns
{
    public class Haurvatat : Djinn.OnAcquireDjinnBase
    {
        public static readonly Haurvatat Instance = new Haurvatat();

        private Haurvatat()
            : base(8)
        {
        }

        protected override GameState OnAcquire(PlayerToken player, GameState state)
        {
            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, state.ScoreTables[player].With(palmTreeValue: 5)));
        }
    }
}
