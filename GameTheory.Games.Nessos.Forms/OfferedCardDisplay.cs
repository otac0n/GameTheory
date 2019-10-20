// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;
    using static FormsRunner.Shared.Controls;

    public class OfferedCardDisplay : Display
    {
        private CardDisplay cardDisplay = new CardDisplay(showAsUncertain: true);

        /// <inheritdoc/>
        public override bool CanDisplay(Scope scope, Type type, object value) => typeof(OfferedCard).IsAssignableFrom(type);

        /// <inheritdoc/>
        protected override Control Update(Control control, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
        {
            var offeredCard = (OfferedCard)value;

            if (control is FlowLayoutPanel flowPanel && flowPanel.Tag == this && flowPanel.Controls.Count <= 2)
            {
                flowPanel.SuspendLayout();
            }
            else
            {
                flowPanel = MakeFlowPanel(tag: this);
                flowPanel.FlowDirection = FlowDirection.TopDown;
                flowPanel.SuspendLayout();
            }

            Display.Update(
                flowPanel.Controls.Count > 0 ? flowPanel.Controls[0] : null,
                scope.Extend(nameof(OfferedCard.SourcePlayer)),
                typeof(PlayerToken),
                offeredCard.SourcePlayer,
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

            this.cardDisplay.UpdateWithAction(
                flowPanel.Controls.Count > 1 ? flowPanel.Controls[1] : null,
                scope.Extend(nameof(OfferedCard.ClaimedCard)),
                typeof(Card),
                offeredCard.ClaimedCard,
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
