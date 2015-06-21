// -----------------------------------------------------------------------
// <copyright file="Boaz.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    public class Boaz : Djinn.OnAcquireDjinnBase
    {
        public static readonly Boaz Instance = new Boaz();

        private Boaz()
            : base(6)
        {
        }

        protected override GameState OnAcquire(PlayerToken owner, GameState state)
        {
            var assassinationTable = state.AssassinationTables[owner];

            return state.With(
                assassinationTables: state.AssassinationTables.SetItem(owner, assassinationTable.With(hasProtection: true)));
        }
    }
}
