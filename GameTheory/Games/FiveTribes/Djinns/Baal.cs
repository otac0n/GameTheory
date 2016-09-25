// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Linq;

    /// <summary>
    /// Each time someone gets a Djinn, collect 1 GC if it's you, 2 GCSs if it's an opponent.
    /// </summary>
    public class Baal : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Baal"/>.
        /// </summary>
        public static readonly Baal Instance = new Baal();

        private Baal()
            : base(6)
        {
        }

        /// <inheritdoc />
        public override string Name
        {
            get { return "Ba'al"; }
        }

        /// <inheritdoc />
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
