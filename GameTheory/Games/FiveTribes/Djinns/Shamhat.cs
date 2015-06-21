// -----------------------------------------------------------------------
// <copyright file="Shamhat.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    public class Shamhat : Djinn.OnAcquireDjinnBase
    {
        public static readonly Shamhat Instance = new Shamhat();

        private Shamhat()
            : base(6)
        {
        }

        protected override GameState OnAcquire(PlayerToken player, GameState state)
        {
            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, state.ScoreTables[player].With(elderValue: 4)));
        }
    }
}
