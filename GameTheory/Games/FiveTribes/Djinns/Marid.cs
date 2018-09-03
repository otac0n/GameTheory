// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;

    /// <summary>
    /// Each time a Meeple is dropped on one of your Tiles during a Move, collect 1 GC if you did the Move; 2 GCs if one of your opponents did.
    /// </summary>
    public sealed class Marid : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Marid"/>.
        /// </summary>
        public static readonly Marid Instance = new Marid();

        private Marid()
            : base(6)
        {
        }

        /// <inheritdoc />
        public override string Name => Resources.Marid;

        /// <inheritdoc />
        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            if (oldState == null)
            {
                throw new ArgumentNullException(nameof(oldState));
            }

            if (newState == null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            if (oldState.Phase == Phase.MoveMeeples && oldState.LastPoint != newState.LastPoint && newState.Sultanate[Sultanate.Size.IndexOf(newState.LastPoint)].Owner == owner)
            {
                var inventory = newState.Inventory[owner];
                newState = newState.With(
                    inventory: newState.Inventory.SetItem(owner, inventory.With(goldCoins: inventory.GoldCoins + (oldState.ActivePlayer == owner ? 1 : 2))));
            }

            return newState;
        }
    }
}
