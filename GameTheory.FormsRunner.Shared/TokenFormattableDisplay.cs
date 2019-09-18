// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class TokenFormattableDisplay : Display
    {
        private TokenFormattableDisplay()
        {
        }

        public static TokenFormattableDisplay Instance { get; } = new TokenFormattableDisplay();

        public override bool CanDisplay(string path, string name, Type type, object value) => value is ITokenFormattable;

        public override Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays)
        {
            var flowPanel = ObjectGraphEditor.MakeFlowPanel();

            flowPanel.SuspendLayout();

            var i = 0;
            foreach (var token in (value as ITokenFormattable).FormatTokens)
            {
                var itemName = $"FormatTokens[{i}]";
                var itemControl = ObjectGraphEditor.MakeDisplay(
                    path + "." + itemName,
                    itemName,
                    token is null ? typeof(object) : token.GetType(),
                    token,
                    overrideDisplays);
                flowPanel.Controls.Add(itemControl);
                i++;
            }

            flowPanel.ResumeLayout();

            return flowPanel;
        }
    }
}
