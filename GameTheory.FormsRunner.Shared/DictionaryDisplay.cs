// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;
    using System.Windows.Forms;

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

        public override Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays)
        {
            var typeArguments = type.GetGenericArguments();
            var keyType = typeArguments[0];
            var valueType = typeArguments[1];

            var tablePanel = ObjectGraphEditor.MakeTablePanel(1, 2);

            tablePanel.SuspendLayout();

            var keys = type.GetProperty("Keys", BindingFlags.Public | BindingFlags.Instance).GetValue(value);
            var valueProperty = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);

            var i = 0;
            foreach (var key in (IEnumerable)keys)
            {
                var keyName = $"Keys[{i}]";
                var valueName = $"[{key}]";
                var keyControl = ObjectGraphEditor.MakeDisplay(
                    path + "." + keyName,
                    keyName,
                    keyType,
                    key,
                    overrideDisplays);
                var valueControl = ObjectGraphEditor.MakeDisplay(
                    path + valueName,
                    valueName,
                    valueType,
                    valueProperty.GetValue(value, new[] { key }),
                    overrideDisplays);
                tablePanel.Controls.Add(keyControl, 0, i);
                tablePanel.Controls.Add(valueControl, 1, i);
                i++;
            }

            tablePanel.ResumeLayout();

            return tablePanel;
        }
    }
}
