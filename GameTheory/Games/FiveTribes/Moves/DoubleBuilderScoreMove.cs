﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to double the amount of Gold Coins (GCs) the active player's <see cref="Meeple.Builder">Builders</see> get this turn.
    /// </summary>
    public class DoubleBuilderScoreMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleBuilderScoreMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public DoubleBuilderScoreMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => "Double the amout of GCs your Builders get this turn";

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var scoreTable = state.ScoreTables[player];

            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, scoreTable.With(builderMultiplier: scoreTable.BuilderMultiplier * 2)));
        }
    }
}
