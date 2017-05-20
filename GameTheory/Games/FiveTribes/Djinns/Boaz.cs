// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;

    /// <summary>
    /// Your Elders and Viziers are protected from Assassins.
    /// </summary>
    public class Boaz : OnAcquireDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Boaz"/>.
        /// </summary>
        public static readonly Boaz Instance = new Boaz();

        private Boaz()
            : base(6)
        {
        }

        /// <inheritdoc />
        protected override GameState OnAcquire(PlayerToken owner, GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var assassinationTable = state.AssassinationTables[owner];

            return state.With(
                assassinationTables: state.AssassinationTables.SetItem(owner, assassinationTable.With(hasProtection: true)));
        }
    }
}
