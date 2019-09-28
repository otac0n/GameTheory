// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System.Windows.Forms;

    public class Controls
    {
        public static FlowLayoutPanel MakeFlowPanel(object tag = null) => new FlowLayoutPanel
        {
            AutoSize = true,
            Margin = Padding.Empty,
            Tag = tag,
        };

        public static Label MakeLabel(string text, object tag = null) => new Label
        {
            Text = text,
            AutoSize = true,
            Margin = Padding.Empty,
            Tag = tag,
        };

        public static TableLayoutPanel MakeTablePanel(int rows, int columns, object tag = null) => new TableLayoutPanel
        {
            AutoSize = true,
            RowCount = rows,
            ColumnCount = columns,
            Margin = Padding.Empty,
            Tag = tag,
        };
    }
}
