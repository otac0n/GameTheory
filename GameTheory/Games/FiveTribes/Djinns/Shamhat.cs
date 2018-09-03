// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;

    /// <summary>
    /// At game end, each of your Elders is worth 4 VPs instead of 2.
    /// </summary>
    public sealed class Shamhat : OnAcquireDjinnBase
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
        public override string Name => Resources.Shamhat;

        /// <inheritdoc />
        protected override GameState OnAcquire(PlayerToken owner, GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return state.With(
                scoreTables: state.ScoreTables.SetItem(owner, state.ScoreTables[owner].With(elderValue: 4)));
        }
    }
}
