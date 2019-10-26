// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;
    using GameTheory.FormsRunner.Shared.Displays;
    using static FormsRunner.Shared.Controls;

    public class DeckDisplay : Display
    {
        /// <inheritdoc/>
        public override bool CanDisplay(Scope scope, Type type, object value) => scope.Name == nameof(GameState.Deck) && typeof(EnumCollection<Card>).IsAssignableFrom(type);

        /// <inheritdoc/>
        protected override Control Update(Control control, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
        {
            var deck = (EnumCollection<Card>)value;

            if (control is FlowLayoutPanel flowPanel && flowPanel.Tag == this && flowPanel.Controls.Count <= 2)
            {
                flowPanel.SuspendLayout();
            }
            else
            {
                flowPanel = MakeFlowPanel(tag: this);
                flowPanel.FlowDirection = FlowDirection.LeftToRight;
                flowPanel.SuspendLayout();
            }

            PrimitiveDisplay.Instance.Update(
                flowPanel.Controls.Count > 0 ? flowPanel.Controls[0] : null,
                scope.Extend(nameof(deck.Count), deck.Count),
                typeof(string),
                $"{deck.Count} {SharedResources.Times}",
                displays,
                (oldControl, newControl) =>
                {
                    if (oldControl != null)
                    {
                        flowPanel.Controls.Remove(oldControl);
                        oldControl.Dispose();
                    }

                    if (newControl != null)
                    {
                        flowPanel.Controls.Add(newControl);
                        flowPanel.Controls.SetChildIndex(newControl, 0);
                    }
                });

            CardDisplay.Default.Update(
                flowPanel.Controls.Count > 1 ? flowPanel.Controls[1] : null,
                scope.Extend("Top", null),
                typeof(Card),
                Card.None,
                displays,
                (oldControl, newControl) =>
                {
                    if (oldControl != null)
                    {
                        flowPanel.Controls.Remove(oldControl);
                        oldControl.Dispose();
                    }

                    if (newControl != null)
                    {
                        flowPanel.Controls.Add(newControl);
                        flowPanel.Controls.SetChildIndex(newControl, 1);
                    }
                });

            flowPanel.ResumeLayout();

            return flowPanel;
        }
    }
}
