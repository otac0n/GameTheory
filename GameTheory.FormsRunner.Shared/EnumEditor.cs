// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    public class EnumEditor : Editor
    {
        private EnumEditor()
        {
        }

        public static EnumEditor Instance { get; } = new EnumEditor();

        public override bool CanEdit(string path, string name, Type type, object value) => type.IsEnum;

        protected override Control Update(Control control, string path, string name, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set)
        {
            var values = Enum.GetValues(type).Cast<object>().ToArray();
            var comboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Tag = this,
            };
            comboBox.Items.AddRange(values);
            comboBox.SelectedItem = value;
            comboBox.AddMargin(right: ErrorIconPadding);
            comboBox.SelectedValueChanged += (_, a) =>
            {
                setError(comboBox, null);
                set(comboBox.SelectedItem, true);
            };

            set(comboBox.SelectedItem, true);

            return errorControl = comboBox;
        }
    }
}
