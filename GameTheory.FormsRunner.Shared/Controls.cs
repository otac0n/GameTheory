// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System.Windows.Forms;

    public static class Controls
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

        public static Control AddMargin(this Control control, int left = 0, int top = 0, int right = 0, int bottom = 0)
        {
            control.Margin = new Padding(
                control.Margin.Left + left,
                control.Margin.Top + top,
                control.Margin.Right + right,
                control.Margin.Bottom + bottom);
            return control;
        }
    }
}
