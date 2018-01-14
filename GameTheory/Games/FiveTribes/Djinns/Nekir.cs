// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;

    /// <summary>
    /// Each time Assassins kill Meeple(s), collect 1 GC if you did the Killing; 2 GCs if an opponent did.
    /// </summary>
    public sealed class Nekir : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Nekir"/>.
        /// </summary>
        public static readonly Nekir Instance = new Nekir();

        private Nekir()
            : base(6)
        {
        }

        /// <inheritdoc />
        public override GameState HandleAssassination(PlayerToken owner, GameState state, Point point, EnumCollection<Meeple> kill)
        {
            return this.HandleAssassination(owner, state, owner, kill);
        }

        /// <inheritdoc />
        public override GameState HandleAssassination(PlayerToken owner, GameState state, PlayerToken victim, EnumCollection<Meeple> kill)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (kill == null)
            {
                throw new ArgumentNullException(nameof(kill));
            }

            var inventory = state.Inventory[owner];
            var s1 = state.With(
                inventory: state.Inventory.SetItem(owner, inventory.With(goldCoins: inventory.GoldCoins + kill.Count * (state.ActivePlayer == owner ? 1 : 2))));

            return s1;
        }
    }
}
