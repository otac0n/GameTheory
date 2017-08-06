﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;

    public class YieldUpgradeMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YieldUpgradeMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public YieldUpgradeMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => "Yield Upgrade";

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            yield return new YieldUpgradeMove(state);
        }

        /// <inheritdoc />
        internal override GameState Apply(GameState state)
        {
            state = state.With(
                phase: Phase.Play);

            return base.Apply(state);
        }
    }
}
