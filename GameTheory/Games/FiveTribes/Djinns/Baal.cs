namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Linq;

    public class Baal : Djinn
    {
        public static readonly Baal Instance = new Baal();

        private Baal()
            : base(6)
        {
        }

        public override string Name
        {
            get { return "Ba'al"; }
        }

        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            foreach (var player in oldState.Players)
            {
                var newDjinns = newState.Inventory[player].Djinns.Except(oldState.Inventory[player].Djinns).Except(this).Count();

                if (newDjinns > 0)
                {
                    var inventory = newState.Inventory[owner];
                    newState = newState.With(
                        inventory: newState.Inventory.SetItem(owner, inventory.With(goldCoins: inventory.GoldCoins + (player == owner ? 1 : 2) * newDjinns)));
                }
            }

            return newState;
        }
    }
}
