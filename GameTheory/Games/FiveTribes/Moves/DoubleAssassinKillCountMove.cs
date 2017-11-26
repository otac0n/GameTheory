// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to let assassins kill a second <see cref="Meeple"/>.
    /// </summary>
    public sealed class DoubleAssassinKillCountMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleAssassinKillCountMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public DoubleAssassinKillCountMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Double the number of meeples your Assassins kill this turn" };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var assassinationTable = state.AssassinationTables[player];

            return state.With(
                assassinationTables: state.AssassinationTables.SetItem(player, assassinationTable.With(killCount: 2)));
        }
    }
}
