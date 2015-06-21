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

    /// <summary>
    /// Each time a Palace is placed, collect 1 GC if you did it; 2 GCs if your opponents did.
    /// </summary>
    public class Monkir : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Monkir" />.
        /// </summary>
        public static readonly Monkir Instance = new Monkir();

        private Monkir()
            : base(6)
        {
        }

        /// <inheritdoc />
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
