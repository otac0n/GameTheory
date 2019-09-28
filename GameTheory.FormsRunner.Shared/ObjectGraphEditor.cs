// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using static Controls;

    public static class ObjectGraphEditor
    {

        public delegate bool OverrideEditor(string path, string name, Type type, object value, out Control control, out Control errorControl, out Label label, Action<Control, string> setError, Action<object, bool> set);

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
            else if (type.IsEnum)
            {
                var control = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                };
                control.Items.AddRange(Enum.GetValues(type).Cast<object>().ToArray());
                control.SelectedItem = value;
                PadControls(label, control, 10);
                control.SelectedValueChanged += (_, a) =>
                {
                    setError(control, null);
                    set(control.SelectedItem, true);
                };

                set(control.SelectedItem, true);
                return errorControl = control;
            }
            else
            {
                var noParameters = new InitializerParameter[0];
                var nullValues = new[] { new { order = 0, selection = new InitializerSelection("(null)", args => null, noParameters) } }.ToList();
                nullValues.RemoveAll(_ => type.IsValueType);
                var constructors = from constructor in type.GetConstructors()
                                   let parameters = constructor.GetParameters().Select(p => new InitializerParameter(p.Name, p.ParameterType, p.HasDefaultValue ? p.DefaultValue : null)).ToArray()
                                   let text = parameters.Length == 0 ? "Default Instance" : $"Specify {string.Join(", ", parameters.Select(p => p.Name))}"
                                   let accessor = new Func<object[], object>(args => constructor.Invoke(args))
                                   select new { order = parameters.Length == 0 ? 1 : 3, selection = new InitializerSelection(text, accessor, parameters) };
                var staticProperties = from staticProperty in type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                                       where staticProperty.PropertyType == type
                                       let accessor = new Func<object[], object>(_ => staticProperty.GetValue(null))
                                       select new { order = 2, selection = new InitializerSelection(staticProperty.Name, accessor, noParameters) };
                var rootOptions = nullValues.Concat(constructors).Concat(staticProperties).OrderBy(s => s.order).Select(s => s.selection).ToArray();

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
                    constructorList.SelectedIndex = Math.Min(constructorList.Items.Count - 1, nullValues.Count);
                }

                var control = MakeTablePanel(2, 1);
                control.Controls.Add(constructorList, 0, 0);
                control.Controls.Add(propertiesTable, 0, 1);

                // TODO: Dispose controls when control is disposed.
                errorControl = constructorList;
                return control;
            }
        }

        public static string Extend(string path, string name) => string.IsNullOrEmpty(path) ? name : $"{path}.{name}";

        private static void PadControls(Label label, Control control, int labelTop)
        {
            const int ErrorIconPadding = 32;
            label.Margin = new Padding(label.Margin.Left, label.Margin.Top + labelTop, label.Margin.Right, label.Margin.Bottom);
            control.Margin = new Padding(control.Margin.Left, control.Margin.Top, control.Margin.Right + ErrorIconPadding, control.Margin.Bottom);
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
    }
}
