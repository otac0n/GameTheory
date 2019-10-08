// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using GameTheory.Catalogs;
    using GameTheory.ConsoleRunner.Properties;
    using GameTheory.ConsoleRunner.Shared;
    using GameTheory.ConsoleRunner.Shared.Catalogs;
    using Unity;
    using Unity.ServiceLocation;

    internal class Program
    {
        static Program()
        {
            var container = new UnityContainer();
            container.RegisterFactory(typeof(string), null, (c, type, key) =>
            {
                return Environment.CurrentDirectory;
            });
            var serviceLocator = new UnityServiceLocator(container);
            ConsoleRendererCatalog = PluginLoader.LoadCatalogs<IConsoleRendererCatalog>(c => new CompositeConsoleRendererCatalog(c), serviceLocator: serviceLocator);
            GameCatalog = PluginLoader.LoadGameCatalogs(serviceLocator: serviceLocator);
            PlayerCatalog = PluginLoader.LoadPlayerCatalogs(serviceLocator: serviceLocator);
        }

        /// <summary>
        /// Gets the shared static player catalong for the application.
        /// </summary>
        public static IConsoleRendererCatalog ConsoleRendererCatalog { get; }

        /// <summary>
        /// Gets the shared static game catalog for the application.
        /// </summary>
        public static IGameCatalog GameCatalog { get; }

        /// <summary>
        /// Gets the shared static player catalong for the application.
        /// </summary>
        public static IPlayerCatalog PlayerCatalog { get; }

        private static object ConstructType(Type type, Func<ParameterInfo, object> getParameter = null)
        {
            getParameter = getParameter ?? (p => GetArgument(p));

            var constructors = from constructor in type.GetConstructors()
                               let parameters = constructor.GetParameters()
                               let name = parameters.Length == 0 ? "(default)" : string.Join(", ", parameters.Select(p => p.Name))
                               let accessor = new Func<object>(() => constructor.Invoke(constructor.GetParameters().Select(getParameter).ToArray()))
                               select ConsoleInteraction.MakeChoice(name, accessor);
            var staticProperties = from staticProperty in type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                                   where staticProperty.PropertyType == type
                                   let accessor = new Func<object>(() => staticProperty.GetValue(null))
                                   select ConsoleInteraction.MakeChoice(staticProperty.Name, accessor);

            var sources = constructors.Concat(staticProperties).ToList();
            var source = ConsoleInteraction.Choose(sources, skipMessage: _ => Resources.SingleConstructor);
            return source.Value();
        }

        private static object GetArgument(ParameterInfo parameter)
        {
            Console.Write(Resources.ParameterName, parameter.Name);

            if (parameter.ParameterType != typeof(string) &&
                parameter.ParameterType != typeof(bool) &&
                parameter.ParameterType != typeof(int) &&
                !parameter.ParameterType.IsEnum)
            {
                Console.WriteLine();

                return ConstructType(parameter.ParameterType);
            }

            var range = parameter.GetCustomAttribute<RangeAttribute>();
            if (range != null && (!(range.Minimum is int) || parameter.ParameterType != typeof(int)))
            {
                range = null;
            }

            if (parameter.HasDefaultValue)
            {
                if (range != null)
                {
                    Console.WriteLine(Resources.RangeWithDefault, range.Minimum, range.Maximum, parameter.DefaultValue);
                }
                else
                {
                    Console.Write(Resources.DefaultValue, parameter.DefaultValue);
                }
            }
            else if (range != null)
            {
                Console.WriteLine(Resources.Range, range.Minimum, range.Maximum);
            }

            Console.WriteLine();

            while (true)
            {
                var line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                {
                    if (parameter.HasDefaultValue)
                    {
                        return parameter.DefaultValue;
                    }
                    else
                    {
                        Console.WriteLine(Resources.NoDefault);
                        continue;
                    }
                }

                if (parameter.ParameterType == typeof(string))
                {
                    return line;
                }
                else if (parameter.ParameterType.IsEnum)
                {
                    try
                    {
                        return Enum.Parse(parameter.ParameterType, line, ignoreCase: true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(Resources.InvalidInput, ex.Message);
                    }
                }
                else if (parameter.ParameterType == typeof(int))
                {
                    if (int.TryParse(line, out var selection))
                    {
                        if (range == null || ((int)range.Minimum <= selection && (int)range.Maximum >= selection))
                        {
                            return selection;
                        }
                        else
                        {
                            Console.WriteLine(range.FormatErrorMessage(parameter.Name));
                        }
                    }
                    else
                    {
                        Console.WriteLine(Resources.InvalidInteger);
                    }
                }
                else if (parameter.ParameterType == typeof(bool))
                {
                    switch (line.ToUpperInvariant())
                    {
                        case "Y":
                        case "YES":
                        case "T":
                        case "TRUE":
                            return true;

                        case "N":
                        case "NO":
                        case "F":
                        case "FALSE":
                            return true;

                        default:
                            Console.WriteLine(Resources.InvalidBoolean);
                            break;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            NativeMethods.SetConsoleFont();
            NativeMethods.Maximize();

            var game = ConsoleInteraction.Choose(GameCatalog.AvailableGames);
            var gameType = game.GameStateType;
            var state = ConstructType(gameType);

            typeof(Program).GetMethod(nameof(RunGame), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(game.MoveType).Invoke(null, new object[] { state });

            if (Debugger.IsAttached)
            {
                Console.ReadKey(intercept: true);
            }
        }

        private static void RunGame<TMove>(IGameState<TMove> state)
            where TMove : IMove
        {
            Console.WriteLine(Resources.GamePlayerCount, string.Format(state.Players.Count == 1 ? Resources.SingularPlayer : Resources.PluralPlayers, state.Players.Count));
            var players = PlayerCatalog.FindPlayers(typeof(TMove));
            var rendererType = ConsoleRendererCatalog
                .FindConsoleRenderers<TMove>()
                .OrderBy(t => t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(ToStringConsoleRenderer<>))
                .First();
            var consoleRenderer = (IConsoleRenderer<TMove>)Activator.CreateInstance(rendererType);
            var font = consoleRenderer.GetType().GetCustomAttributes(inherit: true).OfType<ConsoleFontAttribute>().FirstOrDefault();
            if (font != null)
            {
                NativeMethods.Restore();
                NativeMethods.SetConsoleFont(font.Name, font.XSize, font.YSize);
                NativeMethods.Maximize();
            }

            Console.WriteLine(Resources.StartingState);
            consoleRenderer.Show(state);

            IPlayer<TMove> choosePlayer(PlayerToken playerToken)
            {
                consoleRenderer.Show(state, FormatUtilities.ParseStringFormat(Resources.ChoosePlayer, playerToken));
                Console.WriteLine();
                var player = ConsoleInteraction.Choose(players as IList<ICatalogPlayer> ?? players.ToList());
                return (IPlayer<TMove>)ConstructType(player.PlayerType, p => p.Name == nameof(playerToken) && p.ParameterType == typeof(PlayerToken) ? playerToken : GetArgument(p));
            }

            IPlayer<TMove> getPlayer(PlayerToken playerToken)
            {
                var player = choosePlayer(playerToken);
                player.MessageSent += (obj, args) =>
                {
                    ConsoleInteraction.WithLock(() =>
                    {
                        consoleRenderer.Show(state, FormatUtilities.ParseStringFormat(Resources.PlayerMessaged, playerToken));
                        consoleRenderer.Show(state, args.FormatTokens);
                        Console.WriteLine();
                    });
                };
                return player;
            }

            state = GameUtilities.PlayGame(state, getPlayer, (prevState, move, newState) => ShowMove(newState, move, consoleRenderer), TimeSpan.FromMinutes(5)).Result;
            Console.WriteLine(Resources.FinalState);

            Console.WriteLine();
            consoleRenderer.Show(state);
            Console.WriteLine();

            Console.WriteLine(Resources.Winners);
            var anyWinners = false;
            foreach (var winner in state.GetWinners())
            {
                anyWinners = true;
                consoleRenderer.Show(state, new[] { winner });
                Console.WriteLine();
            }

            if (!anyWinners)
            {
                Console.WriteLine(Resources.None);
            }
        }

        private static void ShowMove<TMove>(IGameState<TMove> state, TMove move, IConsoleRenderer<TMove> consoleRenderer)
            where TMove : IMove
        {
            ConsoleInteraction.WithLock(() =>
            {
                consoleRenderer.Show(state, FormatUtilities.ParseStringFormat(Resources.PlayerMoved, move.PlayerToken));
                Console.WriteLine();

                consoleRenderer.Show(state, move);
                Console.WriteLine();

                Console.WriteLine();
                consoleRenderer.Show(state);
                Console.WriteLine();
            });
        }
    }
}
