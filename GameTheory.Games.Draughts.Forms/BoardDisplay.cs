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
        public override bool CanDisplay(Scope scope, Type type, object value) => type == typeof(GameState);

        /// <inheritdoc/>
        protected override Control Update(Control control, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
        {
            var gameState = (GameState)value;

            if (control is Checkerboard checkerboard)
            {
                checkerboard.GameState = gameState;
            }
            else
            {
                checkerboard = new Checkerboard(gameState, scope.GetPropertyOrDefault(Scope.SharedProperties.PlayerToken, gameState.Players[0]));
            }

            return checkerboard;
        }
    }
}
