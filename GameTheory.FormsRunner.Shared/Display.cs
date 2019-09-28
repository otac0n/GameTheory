// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    public abstract class Display
    {
        private static readonly IList<Display> Displays = new List<Display>
        {
            NullDisplay.Instance,
            PrimitiveDisplay.Instance,
            TokenFormattableDisplay.Instance,
            DictionaryDisplay.Instance,
            ListDisplay.Instance,
            ObjectGraphDisplay.Instance,
        }.AsReadOnly();

        public static Control Update<T>(Control control, string path, string name, T value, IReadOnlyList<Display> displays, Action<Control, Control> update) =>
            Update(control, path, name, typeof(T), value, displays, update);

        public static Control Update(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays, Action<Control, Control> update)
        {
            foreach (var display in displays == null ? Displays : displays.Concat(Displays))
            {
                if (display.CanDisplay(path, name, type, value))
                {
                    return display.UpdateWithAction(control, path, name, type, value, displays, update);
                }
            }

            return null;
        }

        public abstract bool CanDisplay(string path, string name, Type type, object value);

        public Control UpdateWithAction(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays, Action<Control, Control> update = null)
        {
            var newControl = this.Update(control, path, name, type, value, displays);

            if (update != null && !object.ReferenceEquals(control, newControl))
            {
                update.Invoke(control, newControl);
            }

            return newControl;
        }

        protected abstract Control Update(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays);
    }
}
