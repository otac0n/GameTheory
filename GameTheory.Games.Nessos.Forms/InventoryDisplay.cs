// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;
    using GameTheory.FormsRunner.Shared.Displays;
    using static FormsRunner.Shared.Controls;

    public class InventorydDisplay : Display
    {
        /// <inheritdoc/>
        public override bool CanDisplay(Scope scope, Type type, object value) => typeof(Inventory).IsAssignableFrom(type);

        /// <inheritdoc/>
        protected override Control Update(Control control, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
        {
            var inventory = (Inventory)value;

            var ownedCards = inventory.OwnedCards;
            var hand = inventory.Hand;

            if (control is TableLayoutPanel tablePanel && tablePanel.Tag == this && tablePanel.RowCount == 2 && tablePanel.ColumnCount == 2)
            {
                tablePanel.SuspendLayout();
            }
            else
            {
                tablePanel = MakeTablePanel(2, 2, tag: this);
                tablePanel.SuspendLayout();
                tablePanel.Controls.Add(MakeLabel("Owned"), 0, 0);
                tablePanel.Controls.Add(MakeLabel("Hand"), 0, 1);
            }

            ListDisplay.Instance.UpdateWithAction(
                tablePanel.GetControlFromPosition(1, 0),
                scope.Extend(nameof(inventory.OwnedCards)),
                typeof(EnumCollection<Card>),
                ownedCards,
                displays,
                (oldControl, newControl) =>
                {
                    if (oldControl != null)
                    {
                        tablePanel.Controls.Remove(oldControl);
                        oldControl.Dispose();
                    }

                    if (newControl != null)
                    {
                        tablePanel.Controls.Add(newControl, 1, 0);
                    }
                });

            ListDisplay.Instance.UpdateWithAction(
                tablePanel.GetControlFromPosition(1, 1),
                scope.Extend(nameof(inventory.Hand)),
                typeof(EnumCollection<Card>),
                hand,
                displays,
                (oldControl, newControl) =>
                {
                    if (oldControl != null)
                    {
                        tablePanel.Controls.Remove(oldControl);
                        oldControl.Dispose();
                    }

                    if (newControl != null)
                    {
                        tablePanel.Controls.Add(newControl, 1, 1);
                    }
                });

            tablePanel.ResumeLayout();

            return tablePanel;
        }
    }
}
