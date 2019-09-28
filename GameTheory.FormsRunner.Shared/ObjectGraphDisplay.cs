// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using static Controls;

    public class ObjectGraphDisplay : Display
    {
        private ObjectGraphDisplay()
        {
        }

        public static ObjectGraphDisplay Instance { get; } = new ObjectGraphDisplay();

        private static Type MemberValueType(MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo field:
                    return field.FieldType;

                case PropertyInfo property:
                    return property.PropertyType;
            }

            return null;
        }

        private static object GetMemberValue(MemberInfo member, object value)
        {
            switch (member)
            {
                case FieldInfo field:
                    return field.GetValue(field.IsStatic ? null : value);

                case PropertyInfo property:
                    return property.GetValue(property.GetMethod.IsStatic ? null : value);
            }

            return null;
        }

        public override bool CanDisplay(string path, string name, Type type, object value) => value is object;

        protected override Control Update(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays)
        {
            IEnumerable<Type> baseTypes;
            if (type.IsInterface)
            {
                var seen = new HashSet<Type>();

                var queue = new Queue<Type>();
                queue.Enqueue(type);

                while (queue.Count > 0)
                {
                    var @interface = queue.Dequeue();
                    if (seen.Add(@interface))
                    {
                        foreach (var subInterface in @interface.GetInterfaces())
                        {
                            queue.Enqueue(subInterface);
                        }
                    }
                }

                baseTypes = seen;
            }
            else
            {
                baseTypes = new[] { type };
            }

            var fields = baseTypes.SelectMany(bt => bt.GetFields(BindingFlags.Public | BindingFlags.Instance));
            var properties = baseTypes.SelectMany(bt => bt.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead));
            var readableMembers = fields.Cast<MemberInfo>().Concat(properties).ToList();

            var staticFields = baseTypes.SelectMany(bt => bt.GetFields(BindingFlags.Public | BindingFlags.Static));
            var staticProperties = baseTypes.SelectMany(bt => bt.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => p.CanRead));
            var readableStaticMembers = staticFields.Cast<MemberInfo>().Concat(staticProperties).ToList();

            var rowCount = Math.Max(1, readableMembers.Count);

            if (control is TableLayoutPanel propertiesTable && propertiesTable.Tag == this && propertiesTable.ColumnCount == 2)
            {
                propertiesTable.SuspendLayout();

                for (var i = rowCount; i < propertiesTable.RowCount; i++)
                {
                    var keyControl = propertiesTable.GetControlFromPosition(0, i);
                    if (keyControl != null)
                    {
                        propertiesTable.Controls.Remove(keyControl);
                        keyControl.Dispose();
                    }

                    var valueControl = propertiesTable.GetControlFromPosition(0, i);
                    if (valueControl != null)
                    {
                        propertiesTable.Controls.Remove(valueControl);
                        valueControl.Dispose();
                    }
                }

                propertiesTable.RowCount = rowCount;
            }
            else
            {
                propertiesTable = MakeTablePanel(rowCount, 2, tag: this);
                propertiesTable.SuspendLayout();
            }

            for (var i = 0; i < readableMembers.Count; i++)
            {
                var p = i;
                var member = readableMembers[p];

                var oldLabel = propertiesTable.GetControlFromPosition(0, p);
                if (oldLabel is Label label && label.Tag == this)
                {
                    label.Text = member.Name;
                }
                else
                {
                    propertiesTable.Controls.Add(MakeLabel(member.Name, tag: this), 0, p);
                    oldLabel?.Dispose();
                }

                Type memberType;
                object memberValue;
                if (member is FieldInfo field)
                {
                    memberType = field.FieldType;
                    memberValue = field.GetValue(value);
                }
                else if (member is PropertyInfo property)
                {
                    memberType = property.PropertyType;

                    var indexParameters = property.GetIndexParameters();
                    if (indexParameters.Length == 1)
                    {
                        var parameter = indexParameters.Single();
                        if (parameter.Name == "index" && parameter.ParameterType == typeof(int))
                        {
                            var countProperty =
                                readableMembers.FirstOrDefault(y => y.Name == "Count" && MemberValueType(y) == typeof(int)) ??
                                readableMembers.FirstOrDefault(y => y.Name == "Length" && MemberValueType(y) == typeof(int));
                            if (countProperty != null)
                            {
                                var count = (int)GetMemberValue(countProperty, value);
                                ListDisplay.Instance.UpdateWithAction(
                                    propertiesTable.GetControlFromPosition(1, p),
                                    ObjectGraphEditor.Extend(path, member.Name),
                                    member.Name,
                                    memberType,
                                    Enumerable.Range(0, count).Select(argument => property.GetValue(value, new object[] { argument })).ToList(),
                                    displays,
                                    (oldControl, newControl) =>
                                    {
                                        if (oldControl != null)
                                        {
                                            propertiesTable.Controls.Remove(oldControl);
                                            oldControl.Dispose();
                                        }

                                        propertiesTable.Controls.Add(newControl, 1, p);
                                    });

                                continue;
                            }
                        }
                    }
                    else if (indexParameters.Length == 2)
                    {
                        var first = indexParameters[0];
                        var second = indexParameters[1];
                        if (first.Name == "x" && first.ParameterType == typeof(int) && second.Name == "y" && second.ParameterType == typeof(int))
                        {
                            var widthProperty =
                                readableMembers.FirstOrDefault(y => y.Name == "Width" && MemberValueType(y) == typeof(int)) ??
                                readableStaticMembers.FirstOrDefault(y => y.Name == "Width" && MemberValueType(y) == typeof(int));
                            var heightProperty =
                                readableMembers.FirstOrDefault(y => y.Name == "Height" && MemberValueType(y) == typeof(int)) ??
                                readableStaticMembers.FirstOrDefault(y => y.Name == "Height" && MemberValueType(y) == typeof(int));

                            if (widthProperty == null && heightProperty == null)
                            {
                                var sizeProperty =
                                    readableMembers.FirstOrDefault(y => y.Name == "Size" && MemberValueType(y) == typeof(int)) ??
                                    readableStaticMembers.FirstOrDefault(y => y.Name == "Size" && MemberValueType(y) == typeof(int));
                                if (sizeProperty != null)
                                {
                                    widthProperty = sizeProperty;
                                    heightProperty = sizeProperty;
                                }
                            }

                            if (widthProperty != null && heightProperty != null)
                            {
                                var width = (int)GetMemberValue(widthProperty, value);
                                var height = (int)GetMemberValue(heightProperty, value);
                                var range = from x in Enumerable.Range(0, width)
                                            from y in Enumerable.Range(0, height)
                                            select new { x, y };

                                var tablePanel = MakeTablePanel(Math.Max(1, height), Math.Max(1, width));

                                tablePanel.SuspendLayout();

                                foreach (var pair in range)
                                {
                                    var itemControl = Display.Update(
                                        null,
                                        ObjectGraphEditor.Extend(path, member.Name) + $"[{pair.x}, {pair.y}]",
                                        member.Name,
                                        memberType,
                                        property.GetValue(value, new object[] { pair.x, pair.y }),
                                        displays,
                                        (oldControl, newControl) => { });

                                    tablePanel.Controls.Add(itemControl);
                                }

                                tablePanel.ResumeLayout();

                                var oldValueControl = propertiesTable.GetControlFromPosition(1, p);
                                if (oldValueControl != null)
                                {
                                    propertiesTable.Controls.Remove(oldValueControl);
                                    oldValueControl.Dispose();
                                }

                                propertiesTable.Controls.Add(tablePanel, 1, p);
                                continue;
                            }
                        }
                    }

                    if (indexParameters.Length > 0)
                    {
                        memberValue = null; // !!!!!
                    }
                    else
                    {
                        memberValue = property.GetValue(value);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                Display.Update(
                    propertiesTable.GetControlFromPosition(1, p),
                    ObjectGraphEditor.Extend(path, member.Name),
                    member.Name,
                    memberType,
                    memberValue,
                    displays,
                    (oldControl, newControl) =>
                    {
                        if (oldControl != null)
                        {
                            propertiesTable.Controls.Remove(oldControl);
                            oldControl.Dispose();
                        }

                        propertiesTable.Controls.Add(newControl, 1, p);
                    });
            }

            propertiesTable.ResumeLayout();

            return propertiesTable;
        }
    }
}
