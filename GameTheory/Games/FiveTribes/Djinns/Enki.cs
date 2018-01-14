// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;
    using GameTheory.Games.FiveTribes.Tiles;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> to place 1 Palm Tree on any Oasis.
    /// </summary>
    public sealed class Enki : PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Enki"/>.
        /// </summary>
        public static readonly Enki Instance = new Enki();

        private Enki()
            : base(8, Cost.OneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override InterstitialState GetInterstitialState() => new ChoosingSquare();

        private class ChoosingSquare : InterstitialState
        {
            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                return from i in Enumerable.Range(0, Sultanate.Width * Sultanate.Height)
                       where state.Sultanate[i].Tile is Oasis
                       select new PlacePalmTreeMove(state, i);
            }
        }
    }
}
