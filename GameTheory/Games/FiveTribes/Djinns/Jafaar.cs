// -----------------------------------------------------------------------
// <copyright file="Jafaar.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    /// <summary>
    /// At game end, each Vizier you hold is worth 3 VPs instead of 1.
    /// </summary>
    public class Jafaar : Djinn.OnAcquireDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Jafaar"/>.
        /// </summary>
        public static readonly Jafaar Instance = new Jafaar();

        private Jafaar()
            : base(6)
        {
        }

        /// <inheritdoc />
        protected override GameState OnAcquire(PlayerToken player, GameState state)
        {
            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, state.ScoreTables[player].With(vizierValue: 3)));
        }
    }
}
