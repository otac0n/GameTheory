// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;

    public class CardDisplay : Display
    {
        /// <inheritdoc/>
        public override bool CanDisplay(string path, string name, Type type, object value) => typeof(Card).IsAssignableFrom(type);

        /// <inheritdoc/>
        public override Control Update(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays)
        {
            var card = (Card)value;

            if (control is CardControl cardControl)
            {
                cardControl.Card = card;
            }
            else
            {
                cardControl = new CardControl(card);
            }

            return cardControl;
        }
    }
}
