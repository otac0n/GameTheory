// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;
    using GameTheory.Games.SevenDragons.Cards;

    public class TableDisplay : Display
    {
        /// <inheritdoc/>
        public override bool CanDisplay(string path, string name, Type type, object value) => type == typeof(ImmutableDictionary<Point, DragonCard>);

        /// <inheritdoc/>
        protected override Control Update(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays)
        {
            var table = (ImmutableDictionary<Point, DragonCard>)value;

            if (control is TableControl tableControl)
            {
                tableControl.Cards = table;
            }
            else
            {
                tableControl = new TableControl(table);
            }

            return tableControl;
        }
    }
}
