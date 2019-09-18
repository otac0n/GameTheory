// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Windows.Forms;

    public class ListDisplay : Display
    {
        private static HashSet<Type> Types = new HashSet<Type>
        {
            typeof(List<>),
            typeof(IList<>),
            typeof(IReadOnlyList<>),
            typeof(IReadOnlyCollection<>),
            typeof(ImmutableList<>),
            typeof(IImmutableList<>),
            typeof(ImmutableArray<>),
        };

        private ListDisplay()
        {
        }

        public static ListDisplay Instance { get; } = new ListDisplay();

        public override bool CanDisplay(string path, string name, Type type, object value)
        {
            if (type.IsArray)
            {
                return true;
            }

            if (type.IsConstructedGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                return Types.Contains(genericType);
            }

            return false;
        }

        public override Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays)
        {
            var elementType = type.IsArray
                ? type.GetElementType()
                : type.GetGenericArguments().Single();

            var flowPanel = ObjectGraphEditor.MakeFlowPanel();

            flowPanel.SuspendLayout();

            var showVertical = true;
            var i = 0;
            foreach (var item in (IEnumerable)value)
            {
                var itemName = $"[{i}]";
                var itemPath = path + itemName;

                showVertical = showVertical && this.CanDisplay(itemPath, itemName, elementType, item);

                var itemControl = ObjectGraphEditor.MakeDisplay(
                    itemPath,
                    itemName,
                    elementType,
                    item,
                    overrideDisplays);
                flowPanel.Controls.Add(itemControl);
                i++;
            }

            if (showVertical)
            {
                flowPanel.FlowDirection = FlowDirection.TopDown;
            }

            flowPanel.ResumeLayout();

            return flowPanel;
        }
    }
}
