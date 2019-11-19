// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Editors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using GameTheory.Catalogs;
    using GameTheory.FormsRunner.Shared.Properties;
    using static Controls;

    public class ObjectGraphEditor : Editor
    {
        private ObjectGraphEditor()
        {
        }

        public delegate bool OverrideEditor(Scope scope, Type type, object value, out Control control, out Control errorControl, out Label label, Action<Control, string> setError, Action<object, bool> set);

        public static ObjectGraphEditor Instance { get; } = new ObjectGraphEditor();

        public override bool CanEdit(Scope scope, Type type, object value) => true;

        public Control Update(Control control, Scope scope, Type type, Initializer[] initializers, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set, Action<Control, Control> update = null)
        {
            var newControl = this.Update(control, scope, type, initializers, value, out errorControl, editors, setError, set);

            if (update != null && !object.ReferenceEquals(control, newControl))
            {
                update.Invoke(control, newControl);
            }

            return newControl;
        }

        public Control Update(Control control, Scope scope, Type type, Initializer[] initializers, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set)
        {
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
                var constructor = constructorList.SelectedItem as Initializer;
                var parameterCount = constructor.Parameters.Count;
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
                        catch (Exception ex)
                        {
                            valid = false;

                            if (ex is TargetInvocationException)
                            {
                                ex = ex.InnerException;
                            }

                            Control innerErrorControl = null;
                            switch (ex)
                            {
                                case ArgumentException argumentException:
                                    errorControls.TryGetValue(argumentException.ParamName, out innerErrorControl);
                                    break;

                                default:
                                    break;
                            }

                            setError(innerErrorControl ?? constructorList, ex.Message);
                        }
                    }

                    set(parameterValue, valid);
                }

                for (var i = 0; i < parameterCount; i++)
                {
                    var p = i; // Closure variable.
                    var parameter = constructor.Parameters[p];
                    var description = parameter.GetCustomAttribute<DescriptionAttribute>();
                    var displayName = parameter.GetCustomAttribute<DisplayNameAttribute>();
                    var parenthesizePropertyName = parameter.GetCustomAttribute<ParenthesizePropertyNameAttribute>();

                    var item = parameter.HasDefaultValue ? parameter.DefaultValue : null;
                    var innerControl = Editor.FindAndUpdate(
                        null,
                        scope.Extend(parameter.Name, item),
                        parameter.ParameterType,
                        item,
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
                        var parameterName = displayName?.DisplayName ?? parameter.Name;
                        if (parenthesizePropertyName?.NeedParenthesis ?? false)
                        {
                            parameterName = string.Format(Resources.ParameterNameParenthesis, parameterName);
                        }

                        var label = MakeLabel(parameterName, tag: this);

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

            constructorList.Items.AddRange(initializers);
            if (constructorList.Items.Count > 0)
            {
                constructorList.SelectedIndex = 0;
            }

            var tablePanel = MakeTablePanel(2, 1);
            tablePanel.Controls.Add(constructorList, 0, 0);
            tablePanel.Controls.Add(propertiesTable, 0, 1);

            // TODO: Dispose controls when tablePanel is disposed.
            errorControl = constructorList;
            return tablePanel;
        }

        protected override Control Update(Control control, Scope scope, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set)
        {
            var noParameters = new ParameterInfo[0];
            var initializers = (type.IsValueType
                ? type.GetPublicInitializers()
                : new[] { new Initializer(SharedResources.Null, args => null, noParameters) }.Concat(type.GetPublicInitializers())).ToArray();
            return this.Update(control, scope, type, initializers, value, out errorControl, editors, setError, set);
        }
    }
}
