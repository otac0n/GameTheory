// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using GameTheory.Catalogs;
    using GameTheory.ConsoleRunner.Properties;

    internal static class ConsoleInteraction
    {
        private static object @lock = new object();

        public static T Choose<T>(IList<T> options, CancellationToken? cancel = null, Action<T> render = null, Func<T, string> skipMessage = null)
        {
            if (options.Count == 0)
            {
                return default;
            }

            if (skipMessage != null && options.Count == 1)
            {
                var single = options[0];
                Console.WriteLine(skipMessage(single));
                return single;
            }

            Func<string, int> parse;
            if (cancel != null)
            {
                parse = s =>
                {
                    if (cancel.Value.IsCancellationRequested)
                    {
                        Console.WriteLine(Resources.InputDiscarded);
                        throw new OperationCanceledException();
                    }

                    return int.Parse(s);
                };
            }
            else
            {
                parse = int.Parse;
            }

            List(options, render);
            var selection = Prompt(
                string.Format(Resources.ListPrompt, options.Count),
                parse,
                i => i > 0 && i <= options.Count ? null : string.Format(Resources.InvalidListItem, options.Count));

            return options[selection - 1];
        }

        public static object ConstructType(Type type, Func<Parameter, object> getParameter = null) => ConstructType(type.GetPublicInitializers(), getParameter);

        public static object ConstructType(IEnumerable<Initializer> initializers, Func<Parameter, object> getArgument = null)
        {
            getArgument = getArgument ?? (p => GetArgument(p));
            var initializer = ConsoleInteraction.Choose(initializers.ToList(), skipMessage: _ => Resources.SingleConstructor);
            return initializer.Accessor(initializer.Parameters.Select(getArgument).ToArray());
        }

        public static object GetArgument(Parameter parameter)
        {
            Console.Write(parameter.Name);

            if (parameter.ParameterType != typeof(string) &&
                parameter.ParameterType != typeof(bool) &&
                parameter.ParameterType != typeof(int) &&
                !parameter.ParameterType.IsEnum)
            {
                Console.WriteLine();

                return ConstructType(parameter.ParameterType);
            }

            var required = parameter.Validations.OfType<RequiredAttribute>().FirstOrDefault();
            var range = parameter.Validations.OfType<RangeAttribute>().FirstOrDefault();

            if (range != null && (!(range.Minimum is int) || parameter.ParameterType != typeof(int)))
            {
                range = null;
            }

            if (parameter.Default.HasValue)
            {
                Console.Write(' ');
                var defaultDisplay =
                    parameter.Default.Value is null ? Resources.Null :
                    parameter.Default.Value is string d && d == string.Empty ? Resources.Empty :
                    parameter.Default.Value;
                if (range != null)
                {
                    Console.Write(Resources.RangeWithDefault, range.Minimum, range.Maximum, defaultDisplay);
                }
                else
                {
                    Console.Write(Resources.DefaultValue, defaultDisplay);
                }
            }
            else if (range != null)
            {
                Console.Write(' ');
                Console.Write(Resources.Range, range.Minimum, range.Maximum);
            }

            Console.WriteLine();

            if (!string.IsNullOrWhiteSpace(parameter.Description))
            {
                Shared.ConsoleInteraction.WithColor(ConsoleColor.DarkGray, () =>
                {
                    Console.WriteLine(parameter.Description);
                });
            }

            while (true)
            {
                var line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                {
                    if (parameter.Default.HasValue)
                    {
                        return parameter.Default.Value;
                    }
                    else if (required != null && parameter.ParameterType == typeof(string) && required.AllowEmptyStrings == true)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        Console.WriteLine(Resources.NoDefault);
                        continue;
                    }
                }

                object value;

                if (parameter.ParameterType == typeof(string))
                {
                    value = line;
                }
                else if (parameter.ParameterType == typeof(bool))
                {
                    switch (line.ToUpperInvariant())
                    {
                        case "Y":
                        case "YES":
                        case "T":
                        case "TRUE":
                            value = true;
                            break;

                        case "N":
                        case "NO":
                        case "F":
                        case "FALSE":
                            value = false;
                            break;

                        default:
                            Console.WriteLine(Resources.InvalidBoolean);
                            continue;
                    }
                }
                else if (parameter.ParameterType == typeof(int))
                {
                    if (int.TryParse(line, out var selection))
                    {
                        value = selection;
                    }
                    else
                    {
                        Console.WriteLine(Resources.InvalidInteger);
                        continue;
                    }
                }
                else if (parameter.ParameterType.IsEnum)
                {
                    try
                    {
                        value = Enum.Parse(parameter.ParameterType, line, ignoreCase: true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(Resources.InvalidInput, ex.Message);
                        continue;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                var valid = true;
                foreach (var validation in parameter.Validations)
                {
                    if (!validation.IsValid(value))
                    {
                        valid = false;
                        Console.WriteLine(validation.FormatErrorMessage(parameter.Name));
                    }
                }

                if (valid)
                {
                    return value;
                }
            }
        }

        public static void List<T>(IList<T> items, Action<T> render = null)
        {
            render = render ?? new Action<T>(item => Console.Write(item?.ToString()));
            for (var i = 0; i < items.Count; i++)
            {
                Console.Write(Resources.ListItem, i + 1);
                render(items[i]);
                Console.WriteLine();
            }
        }

        public static Choice<T> MakeChoice<T>(string name, T value) => new Choice<T>(name, value);

        public static string Prompt(string prompt, Func<string, string> validate = null)
        {
            return Prompt(prompt, x => x, validate);
        }

        public static T Prompt<T>(string prompt, Func<string, T> parse, Func<T, string> validate = null)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                var line = Console.ReadLine();

                T value;
                try
                {
                    value = parse(line);
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException)
                {
                    Console.WriteLine(Resources.InvalidInput, ex.Message);
                    continue;
                }

                var errorMessage = validate?.Invoke(value);
                if (errorMessage != null)
                {
                    Console.WriteLine(errorMessage);
                    continue;
                }

                return value;
            }
        }

        public static void WithLock(Action action)
        {
            lock (@lock)
            {
                action();
            }
        }

        public static T WithLock<T>(Func<T> action)
        {
            lock (@lock)
            {
                return action();
            }
        }

        public struct Choice<T>
        {
            public Choice(string name, T value)
            {
                this.Name = name;
                this.Value = value;
            }

            public string Name { get; }

            public T Value { get; }

            public override string ToString() => this.Name;
        }
    }
}
