﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderPlusOneElderOrOneSlave"/> to draw the top 3 Djinns from the top of the Djinns pile; keep 1, discard the 2 others.
    /// </summary>
    public sealed class Sibittis : PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Sibittis"/>.
        /// </summary>
        public static readonly Sibittis Instance = new Sibittis();

        private Sibittis()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        public override string Name => Resources.Sibittis;

        /// <inheritdoc />
        protected override InterstitialState InterstitialState => new Paid();

        /// <inheritdoc />
        protected override bool CanGetMoves(GameState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            return base.CanGetMoves(state) && (state.DjinnPile.Count + state.DjinnDiscards.Count) >= 1;
        }

        private class Paid : InterstitialState
        {
            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                yield return new DrawDjinnsMove(state);
            }
        }
    }
}
