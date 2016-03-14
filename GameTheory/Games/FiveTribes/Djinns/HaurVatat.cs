// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    /// <summary>
    /// At game end, each of your Palm Trees is worth 5 VPs instead of 3.
    /// </summary>
    public class Haurvatat : OnAcquireDjinnBase
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
        protected override GameState OnAcquire(PlayerToken owner, GameState state)
        {
            return state.With(
                scoreTables: state.ScoreTables.SetItem(owner, state.ScoreTables[owner].With(palmTreeValue: 5)));
        }
    }
}
