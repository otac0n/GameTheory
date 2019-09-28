// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using static Controls;

    public class DictionaryDisplay : Display
    {
        private static HashSet<Type> Types = new HashSet<Type>
        {
            typeof(Dictionary<,>),
            typeof(IDictionary<,>),
            typeof(IReadOnlyDictionary<,>),
            typeof(ImmutableDictionary<,>),
            typeof(IImmutableDictionary<,>),
            typeof(SortedDictionary<,>),
            typeof(ImmutableSortedDictionary<,>),
            typeof(SplayTreeDictionary<,>),
        };

        private DictionaryDisplay()
        {
        }

        public static DictionaryDisplay Instance { get; } = new DictionaryDisplay();

        public override bool CanDisplay(string path, string name, Type type, object value)
        {
            if (type.IsConstructedGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                return Types.Contains(genericType);
            }

            return false;
        }

        public override Control Update(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays)
        {
            var typeArguments = type.GetGenericArguments();
            var keyType = typeArguments[0];
            var valueType = typeArguments[1];
            var keysProperty = type.GetProperty("Keys", BindingFlags.Public | BindingFlags.Instance).GetValue(value);
            var valueProperty = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
            var keys = ((IEnumerable)keysProperty).Cast<object>().ToList();

            if (control is TableLayoutPanel tablePanel && tablePanel.Tag == this && tablePanel.ColumnCount == 2)
            {
                tablePanel.SuspendLayout();

                for (var i = keys.Count; i < tablePanel.RowCount; i++)
                {
                    var keyControl = tablePanel.GetControlFromPosition(0, i);
                    if (keyControl != null)
                    {
                        tablePanel.Controls.Remove(keyControl);
                        keyControl.Dispose();
                    }

                    var valueControl = tablePanel.GetControlFromPosition(0, i);
                    if (valueControl != null)
                    {
                        tablePanel.Controls.Remove(valueControl);
                        valueControl.Dispose();
                    }
                }

                tablePanel.RowCount = keys.Count;
            }
            else
            {
                tablePanel = MakeTablePanel(keys.Count, 2, tag: this);
                tablePanel.SuspendLayout();
            }

            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var keyName = $"Keys[{i}]";
                var valueName = $"[{key}]";

                Update(
                    tablePanel.GetControlFromPosition(0, i),
                    path + "." + keyName,
                    keyName,
                    keyType,
                    key,
                    displays,
                    (oldControl, newControl) =>
                    {
                        if (oldControl != null)
                        {
                            tablePanel.Controls.Remove(oldControl);
                            oldControl.Dispose();
                        }

                        tablePanel.Controls.Add(newControl, 0, i);
                    });

                Update(
                    tablePanel.GetControlFromPosition(1, i),
                    path + valueName,
                    valueName,
                    valueType,
                    valueProperty.GetValue(value, new[] { key }),
                    displays,
                    (oldControl, newControl) =>
                    {
                        if (oldControl != null)
                        {
                            tablePanel.Controls.Remove(oldControl);
                            oldControl.Dispose();
                        }

                        tablePanel.Controls.Add(newControl, 1, i);
                    });

                i++;
            }

            tablePanel.ResumeLayout();

            return tablePanel;
        }
    }
}
