namespace GameTheory.Games.FiveTribes.Djinns
{
    public class Dhenim : Djinn
    {
        public static readonly Dhenim Instance = new Dhenim();

        private Dhenim()
            : base(6)
        {
        }

        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            foreach (var player in oldState.Players)
            {
                var newViziers = newState.Inventory[player].Meeples[Meeple.Vizier] - oldState.Inventory[player].Meeples[Meeple.Vizier];

                if (newViziers > 0)
                {
                    var inventory = newState.Inventory[owner];
                    newState = newState.With(
                        inventory: newState.Inventory.SetItem(owner, inventory.With(goldCoins: inventory.GoldCoins + (player == owner ? 1 : 2) * newViziers)));
                }
            }

            return newState;
        }
    }
}
