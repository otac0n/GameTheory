// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to let assassins kill a second <see cref="Meeple"/>.
    /// </summary>
    public class DoubleAssassinKillCountMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleAssassinKillCountMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public DoubleAssassinKillCountMove(GameState state)
            : base(state, state.ActivePlayer)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Double the number of meeples your Assassins kill this turn";
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var assassinationTable = state.AssassinationTables[player];

            return state.With(
                assassinationTables: state.AssassinationTables.SetItem(player, assassinationTable.With(killCount: 2)));
        }
    }
}
