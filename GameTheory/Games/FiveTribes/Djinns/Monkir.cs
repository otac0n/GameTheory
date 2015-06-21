// -----------------------------------------------------------------------
// <copyright file="Monkir.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Linq;

    public class Monkir : Djinn
    {
        public static readonly Monkir Instance = new Monkir();

        private Monkir()
            : base(6)
        {
        }

        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            var newPalaces = newState.Sultanate.Sum(s => s.Palaces) - oldState.Sultanate.Sum(s => s.Palaces);

            if (newPalaces > 0)
            {
                var inventory = newState.Inventory[owner];
                newState = newState.With(
                    inventory: newState.Inventory.SetItem(owner, inventory.With(goldCoins: inventory.GoldCoins + (oldState.ActivePlayer == owner ? 1 : 2) * newPalaces)));
            }

            return newState;
        }
    }
}
