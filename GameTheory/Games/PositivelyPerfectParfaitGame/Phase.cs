// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame
{
    /// <summary>
    /// Represents the current phase of the game.
    /// </summary>
    public enum Phase
    {
        /// <summary>
        /// The game is in play.
        /// </summary>
        Play,

        /// <summary>
        /// The active player may choose a scoop from the remaining flavors.
        /// </summary>
        ChooseAFlavor,

        /// <summary>
        /// The active player must trade a scoop with another player.
        /// </summary>
        SwitchAScoop,

        /// <summary>
        /// The active player must choose a scoop to lose.
        /// </summary>
        Oops,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
