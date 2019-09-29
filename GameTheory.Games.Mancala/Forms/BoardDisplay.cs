// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala.Forms
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

            if (control is Board board)
            {
                board.GameState = gameState;
            }
            else
            {
                board = new Board(gameState, gameState.Players[0]);
            }

            return board;
        }
    }
}
