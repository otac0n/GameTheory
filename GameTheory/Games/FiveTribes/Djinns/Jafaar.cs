// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    /// <summary>
    /// At game end, each Vizier you hold is worth 3 VPs instead of 1.
    /// </summary>
    public class Jafaar : OnAcquireDjinnBase
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
        protected override GameState OnAcquire(PlayerToken owner, GameState state)
        {
            return state.With(
                scoreTables: state.ScoreTables.SetItem(owner, state.ScoreTables[owner].With(vizierValue: 3)));
        }
    }
}
