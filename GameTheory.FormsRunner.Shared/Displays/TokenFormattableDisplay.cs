// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Displays
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using static Controls;

    public class TokenFormattableDisplay : Display
    {
        private TokenFormattableDisplay()
        {
        }

        public static TokenFormattableDisplay Instance { get; } = new TokenFormattableDisplay();

        public override bool CanDisplay(Scope scope, Type type, object value) => value is ITokenFormattable;

        protected override Control Update(Control control, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
        {
            var tokens = (value as ITokenFormattable).FormatTokens;
            if (control is FlowLayoutPanel flowPanel && flowPanel.Tag == this)
            {
                flowPanel.SuspendLayout();
                while (flowPanel.Controls.Count > tokens.Count)
                {
                    var oldControl = flowPanel.Controls[tokens.Count];
                    flowPanel.Controls.RemoveAt(tokens.Count);
                    oldControl.Dispose();
                }
            }
            else
            {
                flowPanel = MakeFlowPanel(tag: this);
                flowPanel.SuspendLayout();
            }

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var itemName = $"FormatTokens[{i}]";

                Display.FindAndUpdate(
                    flowPanel.Controls.Count > i ? flowPanel.Controls[i] : null,
                    scope.Extend(itemName, token),
                    token is null ? typeof(object) : token.GetType(),
                    token,
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
                            flowPanel.Controls.SetChildIndex(newControl, i);
                        }
                    });
            }

            flowPanel.ResumeLayout();

            return flowPanel;
        }
    }
}
