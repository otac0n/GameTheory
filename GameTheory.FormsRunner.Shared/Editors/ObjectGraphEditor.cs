// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Editors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using static Controls;

    public class ObjectGraphEditor : Editor
    {
        private ObjectGraphEditor()
        {
        }

        public delegate bool OverrideEditor(Scope scope, Type type, object value, out Control control, out Control errorControl, out Label label, Action<Control, string> setError, Action<object, bool> set);

        public static ObjectGraphEditor Instance { get; } = new ObjectGraphEditor();

        public override bool CanEdit(Scope scope, Type type, object value) => true;

        protected override Control Update(Control control, Scope scope, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set)
        {
            var noParameters = new ParameterInfo[0];
            var nullValues = new[] { new { order = 0, selection = new InitializerSelection("(null)", args => null, noParameters) } }.ToList();
            nullValues.RemoveAll(_ => type.IsValueType);
            var constructors = from constructor in type.GetConstructors()
                               let parameters = constructor.GetParameters()
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
                Tag = this,
            };
            constructorList.AddMargin(right: ErrorIconPadding);

            constructorList.SelectedIndexChanged += (_, a) =>
            {
                var constructor = constructorList.SelectedItem as InitializerSelection;
                var parameterCount = constructor.Parameters.Length;
                propertiesTable.SuspendLayout();
                propertiesTable.Controls.DisposeAndClear();
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

                    var innerControl = Editor.Update(
                        null,
                        scope.Extend(parameter.Name),
                        parameter.ParameterType,
                        parameter.HasDefaultValue ? parameter.DefaultValue : null,
                        out var innerErrorControl,
                        editors,
                        setError,
                        (innerValue, valid) =>
                        {
                            (parameters[p] as IDisposable)?.Dispose();
                            parameters[p] = innerValue;
                            innerValid[p] = valid;
                            if (i >= parameterCount)
                            {
                                Touch();
                            }
                        },
                        (oldControl, newControl) =>
                        {
                            if (oldControl != null)
                            {
                                propertiesTable.Controls.Remove(oldControl);
                                oldControl.Dispose();
                            }

                            if (newControl != null)
                            {
                                propertiesTable.Controls.Add(newControl, 1, p);
                            }
                        });

                    if (!(innerControl is CheckBox))
                    {
                        var label = MakeLabel(parameter.Name, tag: this);

                        switch (innerControl)
                        {
                            case ComboBox comboBox:
                                label.AddMargin(top: 10);
                                break;

                            case TextBox textbox:
                            case NumericUpDown numericUpDown:
                                label.AddMargin(top: 5);
                                break;
                        }

                        propertiesTable.Controls.Add(label, 0, p);
                    }

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

            var tablePanel = MakeTablePanel(2, 1);
            tablePanel.Controls.Add(constructorList, 0, 0);
            tablePanel.Controls.Add(propertiesTable, 0, 1);

            // TODO: Dispose controls when tablePanel is disposed.
            errorControl = constructorList;
            return tablePanel;
        }

        private class InitializerSelection
        {
            public InitializerSelection(string name, Func<object[], object> accessor, ParameterInfo[] parameters)
            {
                this.Name = name;
                this.Accessor = accessor;
                this.Parameters = parameters;
            }

            public Func<object[], object> Accessor { get; }

            public string Name { get; }

            public ParameterInfo[] Parameters { get; }
        }
    }
}
