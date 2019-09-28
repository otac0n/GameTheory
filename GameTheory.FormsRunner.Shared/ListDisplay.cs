// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Windows.Forms;
    using static Controls;

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

        protected override Control Update(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays)
        {
            var elementType = type.IsArray
                ? type.GetElementType()
                : type.GetGenericArguments().Single();
            var values = ((IEnumerable)value).Cast<object>().ToList();

            if (control is FlowLayoutPanel flowPanel && flowPanel.Tag == this)
            {
                flowPanel.SuspendLayout();
                while (flowPanel.Controls.Count > values.Count)
                {
                    var oldControl = flowPanel.Controls[values.Count];
                    flowPanel.Controls.RemoveAt(values.Count);
                    oldControl.Dispose();
                }
            }
            else
            {
                flowPanel = MakeFlowPanel(tag: this);
                flowPanel.SuspendLayout();
            }

            for (var i = 0; i < values.Count; i++)
            {
                var item = values[i];
                var itemName = $"[{i}]";
                var itemPath = path + itemName;

                Display.Update(
                    flowPanel.Controls.Count > i ? flowPanel.Controls[i] : null,
                    itemPath,
                    itemName,
                    elementType,
                    item,
                    displays,
                    (oldControl, newControl) =>
                    {
                        if (oldControl != null)
                        {
                            flowPanel.Controls.Remove(oldControl);
                            oldControl.Dispose();
                        }

                        if (newControl != null)
                        {
                            flowPanel.Controls.Add(newControl);
                            flowPanel.Controls.SetChildIndex(newControl, i);
                        }
                    });
            }

            flowPanel.ResumeLayout();

            return flowPanel;
        }
    }
}
