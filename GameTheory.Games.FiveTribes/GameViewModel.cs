// -----------------------------------------------------------------------
// <copyright file="GameViewModel.cs" company="(none)">
//   Copyright © 2015 Katie Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    /// <summary>
    /// Represents the view for the user.
    /// </summary>
    public class GameViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameViewModel"/> class.
        /// </summary>
        public GameViewModel()
        {
            this.State = new GameState(3);
        }

        /// <summary>
        /// Gets or sets the <see cref="GameState"/>.
        /// </summary>
        public GameState State { get; set; }
    }
}
