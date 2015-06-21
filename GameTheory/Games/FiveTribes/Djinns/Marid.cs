namespace GameTheory.Games.FiveTribes.Djinns
{
    public class Marid : Djinn
    {
        public static readonly Marid Instance = new Marid();

        private Marid()
            : base(6)
        {
        }

        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            if (oldState.Phase == Phase.MoveMeeples && oldState.LastPoint != newState.LastPoint && newState.Sultanate[newState.LastPoint].Owner == owner)
            {
                var inventory = newState.Inventory[owner];
                newState = newState.With(
                    inventory: newState.Inventory.SetItem(owner, inventory.With(goldCoins: inventory.GoldCoins + (oldState.ActivePlayer == owner ? 1 : 2))));
            }

            return newState;
        }
    }
}
