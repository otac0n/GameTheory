namespace GameTheory.Games.FiveTribes.Djinns
{
    public class Nekir : Djinn
    {
        public static readonly Nekir Instance = new Nekir();

        private Nekir()
            : base(6)
        {
        }

        public override GameState HandleAssassination(PlayerToken owner, GameState state0, Point point, EnumCollection<Meeple> kill)
        {
            return this.HandleAssassination(owner, state0, owner, kill);
        }

        public override GameState HandleAssassination(PlayerToken owner, GameState state0, PlayerToken victim, EnumCollection<Meeple> kill)
        {
            var inventory = state0.Inventory[owner];
            var s1 = state0.With(
                inventory: state0.Inventory.SetItem(owner, inventory.With(goldCoins: inventory.GoldCoins + kill.Count * (state0.ActivePlayer == owner ? 1 : 2))));

            return s1;
        }
    }
}
