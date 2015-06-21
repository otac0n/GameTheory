namespace GameTheory.Games.FiveTribes.Djinns
{
    public class Boaz : Djinn.OnAcquireDjinnBase
    {
        public static readonly Boaz Instance = new Boaz();

        private Boaz()
            : base(6)
        {
        }

        protected override GameState OnAcquire(PlayerToken owner, GameState state)
        {
            var assassinationTable = state.AssassinationTables[owner];

            return state.With(
                assassinationTables: state.AssassinationTables.SetItem(owner, assassinationTable.With(hasProtection: true)));
        }
    }
}
