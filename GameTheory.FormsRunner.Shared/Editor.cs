// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared.Editors;

    public abstract class Editor
    {
        protected const int ErrorIconPadding = 32;

        private static readonly IList<Editor> Editors = new List<Editor>
        {
            BoolEditor.Instance,
            StringEditor.Instance,
            Int32Editor.Instance,
            EnumEditor.Instance,
            ObjectGraphEditor.Instance,
        }.AsReadOnly();

        public static string Extend(string path, string name) => string.IsNullOrEmpty(path) ? name : $"{path}.{name}";

        public static Control Update<T>(Control control, string path, string name, T value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set, Action<Control, Control> update = null) =>
            Update(control, path, name, typeof(T), value, out errorControl, editors, setError, set);

        public static Control Update(Control control, string path, string name, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set, Action<Control, Control> update = null)
        {
            foreach (var editor in editors == null ? Editors : editors.Concat(Editors))
            {
                if (editor.CanEdit(path, name, type, value))
                {
                    return editor.UpdateWithAction(control, path, name, type, value, out errorControl, editors, setError, set, update);
                }
            }

            errorControl = null;
            return null;
        }

        public abstract bool CanEdit(string path, string name, Type type, object value);

        public Control UpdateWithAction(Control control, string path, string name, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set, Action<Control, Control> update = null)
        {
            var newControl = this.Update(control, path, name, type, value, out errorControl, editors, setError, set);

            if (update != null && !object.ReferenceEquals(control, newControl))
            {
                update.Invoke(control, newControl);
            }

            return newControl;
        }

        protected abstract Control Update(Control control, string path, string name, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set);
    }
}
