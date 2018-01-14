// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;

    /// <summary>
    /// Each time someone gets a Vizier, collect 1 GC if it's you, 2 GCSs if it's an opponent.
    /// </summary>
    public sealed class Dhenim : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Dhenim"/>.
        /// </summary>
        public static readonly Dhenim Instance = new Dhenim();

        private Dhenim()
            : base(6)
        {
        }

        /// <inheritdoc />
        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            if (oldState == null)
            {
                throw new ArgumentNullException(nameof(oldState));
            }

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
