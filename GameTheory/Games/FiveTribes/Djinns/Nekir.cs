// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    /// <summary>
    /// Each time Assassins kill Meeple(s), collect 1 GC if you did the Killing; 2 GCs if an opponent did.
    /// </summary>
    public class Nekir : Djinn
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
        public override GameState HandleAssassination(PlayerToken owner, GameState state0, Point point, EnumCollection<Meeple> kill)
        {
            return this.HandleAssassination(owner, state0, owner, kill);
        }

        /// <inheritdoc />
        public override GameState HandleAssassination(PlayerToken owner, GameState state0, PlayerToken victim, EnumCollection<Meeple> kill)
        {
            var inventory = state0.Inventory[owner];
            var s1 = state0.With(
                inventory: state0.Inventory.SetItem(owner, inventory.With(goldCoins: inventory.GoldCoins + kill.Count * (state0.ActivePlayer == owner ? 1 : 2))));

            return s1;
        }
    }
}
