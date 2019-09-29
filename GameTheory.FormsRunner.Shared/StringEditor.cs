// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class StringEditor : Editor
    {
        private StringEditor()
        {
        }

        public static StringEditor Instance { get; } = new StringEditor();

        public override bool CanEdit(string path, string name, Type type, object value) => type == typeof(string);

        protected override Control Update(Control control, string path, string name, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set)
        {
            var textBox = new TextBox
            {
                Text = value as string ?? string.Empty,
                Tag = this,
            };
            textBox.AddMargin(right: ErrorIconPadding);
            textBox.TextChanged += (_, a) =>
            {
                setError(textBox, null);
                set(textBox.Text, true);
            };
            set(textBox.Text, true);

            return errorControl = textBox;
        }
    }
}
