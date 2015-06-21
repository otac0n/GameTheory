namespace GameTheory.Games.FiveTribes.Djinns
{
    public class Jafaar : Djinn.OnAcquireDjinnBase
    {
        public static readonly Jafaar Instance = new Jafaar();

        private Jafaar()
            : base(6)
        {
        }

        protected override GameState OnAcquire(PlayerToken player, GameState state)
        {
            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, state.ScoreTables[player].With(vizierValue: 3)));
        }
    }
}
