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
        public override bool CanDisplay(string path, string name, Type type, object value) => type == typeof(GameState);

        /// <inheritdoc/>
        public override Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays)
        {
            var gameState = (GameState)value;
            return new Chessboard(gameState, gameState.Players[0]);
        }
    }
}
