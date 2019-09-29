// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Editors
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class Int32Editor : Editor
    {
        private Int32Editor()
        {
        }

        public static Int32Editor Instance { get; } = new Int32Editor();

        public override bool CanEdit(string path, string name, Type type, object value) => type == typeof(int);

        protected override Control Update(Control control, string path, string name, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set)
        {
            var numericUpDown = new NumericUpDown
            {
                Value = value as int? ?? default,
                Tag = this,
            };
            numericUpDown.AddMargin(right: ErrorIconPadding);
            numericUpDown.ValueChanged += (_, a) =>
            {
                setError(numericUpDown, null);
                set((int)numericUpDown.Value, true);
            };
            set((int)numericUpDown.Value, true);

            return errorControl = numericUpDown;
        }
    }
}
