// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class BoolEditor : Editor
    {
        private BoolEditor()
        {
        }

        public static BoolEditor Instance { get; } = new BoolEditor();

        public override bool CanEdit(string path, string name, Type type, object value) => type == typeof(bool);

        protected override Control Update(Control control, string path, string name, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set)
        {
            var checkBox = new CheckBox
            {
                AutoSize = true,
                Checked = value as bool? ?? default,
                Text = name,
                Tag = this,
            };
            checkBox.AddMargin(right: ErrorIconPadding);
            checkBox.CheckedChanged += (_, a) =>
            {
                setError(checkBox, null);
                set(checkBox.Checked, true);
            };
            set(checkBox.Checked, true);

            return errorControl = checkBox;
        }
    }
}
