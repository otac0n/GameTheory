// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;

    public class BoardDisplay : Display
    {
        /// <inheritdoc/>
        public override bool CanDisplay(string path, string name, Type type, object value) => type == typeof(GameState);

        /// <inheritdoc/>
        protected override Control Update(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays)
        {
            var gameState = (GameState)value;

            if (control is Checkerboard checkerboard)
            {
                checkerboard.GameState = gameState;
            }
            else
            {
                checkerboard = new Checkerboard(gameState, gameState.Players[0]);
            }

            return checkerboard;
        }
    }
}
