namespace GameTheory.Games.FiveTribes.Djinns
{
    public class Shamhat : Djinn.OnAcquireDjinnBase
    {
        public static readonly Shamhat Instance = new Shamhat();

        private Shamhat()
            : base(6)
        {
        }

        protected override GameState OnAcquire(PlayerToken player, GameState state)
        {
            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, state.ScoreTables[player].With(elderValue: 4)));
        }
    }
}
