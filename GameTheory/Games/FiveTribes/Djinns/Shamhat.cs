// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    /// <summary>
    /// At game end, each of your Elders is worth 4 VPs instead of 2.
    /// </summary>
    public class Shamhat : Djinn.OnAcquireDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Shamhat"/>.
        /// </summary>
        public static readonly Shamhat Instance = new Shamhat();

        private Shamhat()
            : base(6)
        {
        }

        /// <inheritdoc />
        protected override GameState OnAcquire(PlayerToken player, GameState state)
        {
            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, state.ScoreTables[player].With(elderValue: 4)));
        }
    }
}
