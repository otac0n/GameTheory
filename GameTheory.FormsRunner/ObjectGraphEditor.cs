// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;

    public static class ObjectGraphEditor
    {
        public static Control MakeEditor(string description, Type type, out Control errorControl, out Control label, Action<Control, string> setError, Action<object, bool> set)
        {
            if (type == typeof(bool))
            {
                label = null;
                var control = new CheckBox
                {
                    Text = description,
                    AutoSize = true,
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

            label = new Label
            {
                Text = description,
                AutoSize = true,
            };

            if (type == typeof(string))
            {
                var control = new TextBox();
                label.Margin = new Padding(label.Margin.Left, label.Margin.Top + 5, label.Margin.Right, label.Margin.Bottom);
                control.Margin = new Padding(control.Margin.Left, control.Margin.Top, control.Margin.Right + 32, control.Margin.Bottom);
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
                var control = new NumericUpDown();
                label.Margin = new Padding(label.Margin.Left, label.Margin.Top + 5, label.Margin.Right, label.Margin.Bottom);
                control.Margin = new Padding(control.Margin.Left, control.Margin.Top, control.Margin.Right + 32, control.Margin.Bottom);
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
                                   let parameters = constructor.GetParameters().Select(p => new InitializerParameter(p.Name, p.ParameterType)).ToArray()
                                   let name = parameters.Length == 0 ? "Default Instance" : $"Specify {string.Join(", ", parameters.Select(p => p.Name))}"
                                   let accessor = new Func<object[], object>(args => constructor.Invoke(args))
                                   select new { order = parameters.Length == 0 ? 0 : 2, selection = new InitializerSelection(name, accessor, parameters) };
                var noParameters = new InitializerParameter[0];
                var staticProperties = from staticProperty in type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                                       where staticProperty.PropertyType == type
                                       let accessor = new Func<object[], object>(_ => staticProperty.GetValue(null))
                                       select new { order = 1, selection = new InitializerSelection(staticProperty.Name, accessor, noParameters) };
                var rootOptions = constructors.Concat(staticProperties).OrderBy(s => s.order).Select(s => s.selection).ToArray();

                var propertiesTable = new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = 2,
                    RowCount = 1,
                };

                var constructorList = new ComboBox
                {
                    DisplayMember = "Name",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                };
                label.Margin = new Padding(label.Margin.Left, label.Margin.Top + 10, label.Margin.Right, label.Margin.Bottom);
                constructorList.Margin = new Padding(constructorList.Margin.Left, constructorList.Margin.Top, constructorList.Margin.Right + 32, constructorList.Margin.Bottom);

                constructorList.SelectedIndexChanged += (_, a) =>
                {
                    var constructor = constructorList.SelectedItem as InitializerSelection;
                    var parameterCount = constructor.Parameters.Length;
                    propertiesTable.Controls.Clear(); // TODO: Dispose controls.
                    propertiesTable.RowCount = Math.Max(1, parameterCount);
                    var parameters = new object[parameterCount];
                    var innerValid = new bool[parameterCount];
                    var disposeControls = new Control[parameterCount];
                    var errorControls = new Dictionary<string, Control>();

                    void Touch()
                    {
                        setError(constructorList, null);

                        object value = null;
                        var valid = innerValid.All(v => v);
                        if (valid)
                        {
                            try
                            {
                                value = constructor.Accessor(parameters);
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

                        set(value, valid);
                    }

                    for (var i = 0; i < parameterCount; i++)
                    {
                        var p = i; // Closure variable.
                        var parameter = constructor.Parameters[p];
                        var innerControl = MakeEditor(parameter.Name, parameter.ParameterType, out var innerErrorControl, out var innerLabel, setError, (value, valid) =>
                        {
                            parameters[p] = value;
                            innerValid[p] = valid;
                            if (i >= parameterCount)
                            {
                                Touch();
                            }
                        });

                        if (innerLabel != null)
                        {
                            propertiesTable.Controls.Add(innerLabel, 0, p);
                        }

                        propertiesTable.Controls.Add(innerControl, 1, p);
                        disposeControls[p] = innerControl;
                        errorControls[parameter.Name] = innerErrorControl;
                    }

                    Touch();
                };

                constructorList.Items.AddRange(rootOptions);
                if (constructorList.Items.Count > 0)
                {
                    constructorList.SelectedIndex = 0;
                }

                var control = new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = 1,
                    RowCount = 2,
                };
                control.Controls.Add(constructorList, 0, 0);
                control.Controls.Add(propertiesTable, 0, 1);

                // TODO: Dispose controls when control is disposed.
                errorControl = constructorList;
                return control;
            }
        }

        private class InitializerSelection
        {
            public InitializerSelection(string name, Func<object[], object> accessor, InitializerParameter[] parameters)
            {
                this.Name = name;
                this.Accessor = accessor;
                this.Parameters = parameters;
            }

            public string Name { get; }

            public Func<object[], object> Accessor { get; }

            public InitializerParameter[] Parameters { get; }
        }

        private class InitializerParameter
        {
            public InitializerParameter(string name, Type parameterType)
            {
                this.Name = name;
                this.ParameterType = parameterType;
            }

            public string Name { get; }

            public Type ParameterType { get; }
        }
    }
}
