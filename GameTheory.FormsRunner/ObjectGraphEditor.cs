// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;

    public static class ObjectGraphEditor
    {
        private static readonly IList<Display> Displays = new List<Display>
        {
            NullDisplay.Instance,
            PrimitiveDisplay.Instance,
            TokenFormattableDisplay.Instance,
            DictionaryDisplay.Instance,
            ListDisplay.Instance,
        }.AsReadOnly();

        public delegate bool OverrideEditor(string path, string name, Type type, object value, out Control control, out Control errorControl, out Label label, Action<Control, string> setError, Action<object, bool> set);

        public static Control MakeDisplay(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays = null)
        {
            foreach (var display in overrideDisplays == null ? Displays : overrideDisplays.Concat(Displays))
            {
                if (display.CanDisplay(path, name, type, value))
                {
                    return display.Create(path, name, type, value, overrideDisplays);
                }
            }

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

            var propertiesTable = MakeTablePanel(Math.Max(1, readableMembers.Count), 2);

            propertiesTable.SuspendLayout();

            for (var i = 0; i < readableMembers.Count; i++)
            {
                var p = i;
                var member = readableMembers[p];
                var label = MakeLabel(member.Name);
                propertiesTable.Controls.Add(label, 0, p);

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
                                readableMembers.FirstOrDefault(y => y.Name == "Count" && t(y) == typeof(int)) ??
                                readableMembers.FirstOrDefault(y => y.Name == "Length" && t(y) == typeof(int));
                            if (countProperty != null)
                            {
                                var range = Enumerable.Range(0, (int)v(countProperty, value));

                                var flowPanel = MakeFlowPanel();

                                flowPanel.SuspendLayout();

                                foreach (var argument in range)
                                {
                                    var itemControl = MakeDisplay(
                                        Extend(path, member.Name) + $"[{argument}]",
                                        member.Name,
                                        memberType,
                                        property.GetValue(value, new object[] { argument }),
                                        overrideDisplays);

                                    flowPanel.Controls.Add(itemControl);
                                }

                                flowPanel.ResumeLayout();
                                propertiesTable.Controls.Add(flowPanel, 1, p);
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
                                readableMembers.FirstOrDefault(y => y.Name == "Width" && t(y) == typeof(int)) ??
                                readableStaticMembers.FirstOrDefault(y => y.Name == "Width" && t(y) == typeof(int));
                            var heightProperty =
                                readableMembers.FirstOrDefault(y => y.Name == "Height" && t(y) == typeof(int)) ??
                                readableStaticMembers.FirstOrDefault(y => y.Name == "Height" && t(y) == typeof(int));

                            if (widthProperty == null && heightProperty == null)
                            {
                                var sizeProperty =
                                    readableMembers.FirstOrDefault(y => y.Name == "Size" && t(y) == typeof(int)) ??
                                    readableStaticMembers.FirstOrDefault(y => y.Name == "Size" && t(y) == typeof(int));
                                if (sizeProperty != null)
                                {
                                    widthProperty = sizeProperty;
                                    heightProperty = sizeProperty;
                                }
                            }

                            if (widthProperty != null && heightProperty != null)
                            {
                                var width = (int)v(widthProperty, value);
                                var height = (int)v(heightProperty, value);
                                var range = from x in Enumerable.Range(0, width)
                                            from y in Enumerable.Range(0, height)
                                            select new { x, y };

                                var tablePanel = MakeTablePanel(Math.Max(1, height), Math.Max(1, width));

                                tablePanel.SuspendLayout();

                                foreach (var pair in range)
                                {
                                    var itemControl = MakeDisplay(
                                        Extend(path, member.Name) + $"[{pair.x}, {pair.y}]",
                                        member.Name,
                                        memberType,
                                        property.GetValue(value, new object[] { pair.x, pair.y }),
                                        overrideDisplays);

                                    tablePanel.Controls.Add(itemControl);
                                }

                                tablePanel.ResumeLayout();
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

                var innerControl = MakeDisplay(
                    Extend(path, member.Name),
                    member.Name,
                    memberType,
                    memberValue,
                    overrideDisplays);

                propertiesTable.Controls.Add(innerControl, 1, p);
            }

            propertiesTable.ResumeLayout();

            return propertiesTable;
        }

        public static Control MakeEditor(string path, string name, Type type, object value, out Control errorControl, out Label label, Action<Control, string> setError, Action<object, bool> set, OverrideEditor overrideEditor = null)
        {
            if (overrideEditor != null && overrideEditor(path, name, type, value, out var overrideControl, out var overrideError, out var overrideLabel, setError, set))
            {
                errorControl = overrideError;
                label = overrideLabel;
                return overrideControl;
            }

            if (type == typeof(bool))
            {
                label = null;
                var control = new CheckBox
                {
                    AutoSize = true,
                    Checked = value as bool? ?? default(bool),
                    Text = name,
                };
                control.Margin = new Padding(control.Margin.Left, control.Margin.Top, control.Margin.Right + 32, control.Margin.Bottom);
                control.CheckedChanged += (_, a) =>
                {
                    setError(control, null);
                    set(control.Checked, true);
                };

                set(control.Checked, true);
                return errorControl = control;
            }

            label = MakeLabel(name);

            if (type == typeof(string))
            {
                var control = new TextBox
                {
                    Text = value as string ?? string.Empty,
                };
                PadControls(label, control, 5);
                control.TextChanged += (_, a) =>
                {
                    setError(control, null);
                    set(control.Text, true);
                };

                set(control.Text, true);
                return errorControl = control;
            }
            else if (type == typeof(int))
            {
                var control = new NumericUpDown
                {
                    Value = value as int? ?? default(int),
                };
                PadControls(label, control, 5);
                control.ValueChanged += (_, a) =>
                {
                    setError(control, null);
                    set((int)control.Value, true);
                };

                set((int)control.Value, true);
                return errorControl = control;
            }
            else
            {
                var constructors = from constructor in type.GetConstructors()
                                   let parameters = constructor.GetParameters().Select(p => new InitializerParameter(p.Name, p.ParameterType, p.HasDefaultValue ? p.DefaultValue : null)).ToArray()
                                   let text = parameters.Length == 0 ? "Default Instance" : $"Specify {string.Join(", ", parameters.Select(p => p.Name))}"
                                   let accessor = new Func<object[], object>(args => constructor.Invoke(args))
                                   select new { order = parameters.Length == 0 ? 0 : 2, selection = new InitializerSelection(text, accessor, parameters) };
                var noParameters = new InitializerParameter[0];
                var staticProperties = from staticProperty in type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                                       where staticProperty.PropertyType == type
                                       let accessor = new Func<object[], object>(_ => staticProperty.GetValue(null))
                                       select new { order = 1, selection = new InitializerSelection(staticProperty.Name, accessor, noParameters) };
                var rootOptions = constructors.Concat(staticProperties).OrderBy(s => s.order).Select(s => s.selection).ToArray();

                var propertiesTable = MakeTablePanel(1, 2);

                var constructorList = new ComboBox
                {
                    DisplayMember = "Name",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                };
                PadControls(label, constructorList, 10);

                constructorList.SelectedIndexChanged += (_, a) =>
                {
                    var constructor = constructorList.SelectedItem as InitializerSelection;
                    var parameterCount = constructor.Parameters.Length;
                    propertiesTable.SuspendLayout();
                    propertiesTable.Controls.Clear(); // TODO: Dispose controls.
                    propertiesTable.RowCount = Math.Max(1, parameterCount);
                    var parameters = new object[parameterCount];
                    var innerValid = new bool[parameterCount];
                    var disposeControls = new Control[parameterCount];
                    var errorControls = new Dictionary<string, Control>();

                    void Touch()
                    {
                        setError(constructorList, null);

                        object parameterValue = null;
                        var valid = innerValid.All(v => v);
                        if (valid)
                        {
                            try
                            {
                                parameterValue = constructor.Accessor(parameters);
                                foreach (var innerErrorControl in errorControls.Values)
                                {
                                    setError(innerErrorControl, null);
                                }
                            }
                            catch (TargetInvocationException ex)
                            {
                                valid = false;
                                var inner = ex.InnerException;
                                Control innerErrorControl = null;
                                switch (inner)
                                {
                                    case ArgumentException argumentException:
                                        errorControls.TryGetValue(argumentException.ParamName, out innerErrorControl);
                                        break;

                                    default:
                                        break;
                                }

                                setError(innerErrorControl ?? constructorList, inner.Message);
                            }
                        }

                        set(parameterValue, valid);
                    }

                    for (var i = 0; i < parameterCount; i++)
                    {
                        var p = i; // Closure variable.
                        var parameter = constructor.Parameters[p];
                        var innerControl = MakeEditor(
                            Extend(path, parameter.Name),
                            parameter.Name,
                            parameter.ParameterType,
                            parameter.DefaultValue,
                            out var innerErrorControl,
                            out var innerLabel,
                            setError,
                            (innerValue, valid) =>
                            {
                                parameters[p] = innerValue;
                                innerValid[p] = valid;
                                if (i >= parameterCount)
                                {
                                    Touch();
                                }
                            },
                            overrideEditor);

                        if (innerLabel != null)
                        {
                            propertiesTable.Controls.Add(innerLabel, 0, p);
                        }

                        propertiesTable.Controls.Add(innerControl, 1, p);
                        disposeControls[p] = innerControl;
                        errorControls[parameter.Name] = innerErrorControl;
                    }

                    propertiesTable.ResumeLayout();

                    Touch();
                };

                constructorList.Items.AddRange(rootOptions);
                if (constructorList.Items.Count > 0)
                {
                    constructorList.SelectedIndex = 0;
                }

                var control = MakeTablePanel(2, 1);
                control.Controls.Add(constructorList, 0, 0);
                control.Controls.Add(propertiesTable, 0, 1);

                // TODO: Dispose controls when control is disposed.
                errorControl = constructorList;
                return control;
            }
        }

        public static FlowLayoutPanel MakeFlowPanel() => new FlowLayoutPanel
        {
            AutoSize = true,
            Margin = Padding.Empty,
        };

        public static Label MakeLabel(string text) => new Label
        {
            Text = text,
            AutoSize = true,
            Margin = Padding.Empty,
        };

        public static TableLayoutPanel MakeTablePanel(int rows, int columns) => new TableLayoutPanel
        {
            AutoSize = true,
            RowCount = rows,
            ColumnCount = columns,
        };

        private static string Extend(string path, string name) => string.IsNullOrEmpty(path) ? name : $"{path}.{name}";

        private static void PadControls(Label label, Control control, int labelTop)
        {
            const int ErrorIconPadding = 32;
            label.Margin = new Padding(label.Margin.Left, label.Margin.Top + labelTop, label.Margin.Right, label.Margin.Bottom);
            control.Margin = new Padding(control.Margin.Left, control.Margin.Top, control.Margin.Right + ErrorIconPadding, control.Margin.Bottom);
        }

        private static Type t(MemberInfo member)
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

        private static object v(MemberInfo member, object value)
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

        public abstract class Display
        {
            public abstract bool CanDisplay(string path, string name, Type type, object value);

            public abstract Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays);
        }

        private class DictionaryDisplay : Display
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

                var tablePanel = MakeTablePanel(1, 2);

                tablePanel.SuspendLayout();

                var keys = type.GetProperty("Keys", BindingFlags.Public | BindingFlags.Instance).GetValue(value);
                var valueProperty = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);

                var i = 0;
                foreach (var key in (IEnumerable)keys)
                {
                    var keyName = $"Keys[{i}]";
                    var valueName = $"[{key}]";
                    var keyControl = MakeDisplay(
                        path + "." + keyName,
                        keyName,
                        keyType,
                        key,
                        overrideDisplays);
                    var valueControl = MakeDisplay(
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

        private class InitializerParameter
        {
            public InitializerParameter(string name, Type parameterType, object defaultValue)
            {
                this.Name = name;
                this.ParameterType = parameterType;
                this.DefaultValue = defaultValue;
            }

            public object DefaultValue { get; }

            public string Name { get; }

            public Type ParameterType { get; }
        }

        private class InitializerSelection
        {
            public InitializerSelection(string name, Func<object[], object> accessor, InitializerParameter[] parameters)
            {
                this.Name = name;
                this.Accessor = accessor;
                this.Parameters = parameters;
            }

            public Func<object[], object> Accessor { get; }

            public string Name { get; }

            public InitializerParameter[] Parameters { get; }
        }

        private class ListDisplay : Display
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

                var flowPanel = MakeFlowPanel();

                flowPanel.SuspendLayout();

                var i = 0;
                foreach (var item in (IEnumerable)value)
                {
                    var itemName = $"[{i}]";
                    var itemControl = MakeDisplay(
                        path + itemName,
                        itemName,
                        elementType,
                        item,
                        overrideDisplays);
                    flowPanel.Controls.Add(itemControl);
                    i++;
                }

                flowPanel.ResumeLayout();

                return flowPanel;
            }
        }

        private class NullDisplay : Display
        {
            private NullDisplay()
            {
            }

            public static NullDisplay Instance { get; } = new NullDisplay();

            public override bool CanDisplay(string path, string name, Type type, object value) => value is null;

            public override Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays) => MakeLabel("(null)");
        }

        private class PrimitiveDisplay : Display
        {
            private PrimitiveDisplay()
            {
            }

            public static PrimitiveDisplay Instance { get; } = new PrimitiveDisplay();

            public override bool CanDisplay(string path, string name, Type type, object value) => type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(Guid);

            public override Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays) => MakeLabel(value.ToString());
        }

        private class TokenFormattableDisplay : Display
        {
            private TokenFormattableDisplay()
            {
            }

            public static TokenFormattableDisplay Instance { get; } = new TokenFormattableDisplay();

            public override bool CanDisplay(string path, string name, Type type, object value) => value is ITokenFormattable;

            public override Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays)
            {
                var flowPanel = MakeFlowPanel();

                flowPanel.SuspendLayout();

                var i = 0;
                foreach (var token in (value as ITokenFormattable).FormatTokens)
                {
                    var itemName = $"FormatTokens[{i}]";
                    var itemControl = MakeDisplay(
                        path + "." + itemName,
                        itemName,
                        token is null ? typeof(object) : token.GetType(),
                        token,
                        overrideDisplays);
                    flowPanel.Controls.Add(itemControl);
                    i++;
                }

                flowPanel.ResumeLayout();

                return flowPanel;
            }
        }
    }
}
