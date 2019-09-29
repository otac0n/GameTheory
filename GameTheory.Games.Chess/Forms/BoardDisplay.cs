// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
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

            if (control is Chessboard chessboard)
            {
                chessboard.GameState = gameState;
            }
            else
            {
                chessboard = new Chessboard(gameState, scope.GetPropertyOrDefault<PlayerToken>("playerToken", gameState.Players[0]));
            }

            return chessboard;
        }
    }
}
