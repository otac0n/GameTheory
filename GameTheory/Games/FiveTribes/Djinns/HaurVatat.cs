// -----------------------------------------------------------------------
// <copyright file="Haurvatat.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    /// <summary>
    /// At game end, each of your Palm Trees is worth 5 VPs instead of 3.
    /// </summary>
    public class Haurvatat : Djinn.OnAcquireDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Haurvatat"/>.
        /// </summary>
        public static readonly Haurvatat Instance = new Haurvatat();

        private Haurvatat()
            : base(8)
        {
        }

        /// <inheritdoc />
        protected override GameState OnAcquire(PlayerToken player, GameState state)
        {
            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, state.ScoreTables[player].With(palmTreeValue: 5)));
        }
    }
}
