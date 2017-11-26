// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Catalogs;
    using GameTheory.ConsoleRunner.ConsoleRenderers;
    using GameTheory.ConsoleRunner.Properties;

    internal class Program
    {
        private static object ConstructType(Type type, Func<ParameterInfo, object> getParameter = null)
        {
            getParameter = getParameter ?? (p => GetArgument(p));
            var constructor = ConsoleInteraction.Choose(type.GetConstructors(), skipMessage: _ => Resources.SingleConstructor);
            var args = constructor.GetParameters().Select(getParameter).ToArray();
            return constructor.Invoke(args);
        }

        private static object GetArgument(ParameterInfo parameter)
        {
            Console.Write(Resources.ParameterName, parameter.Name);

            if (parameter.HasDefaultValue)
            {
                Console.Write(Resources.DefaultValue, parameter.DefaultValue);
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

                if (parameter.ParameterType == typeof(int))
                {
                    if (int.TryParse(line, out int selection))
                    {
                        return selection;
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
            var catalog = GameCatalog.Default;
            var game = ConsoleInteraction.Choose(catalog.AvailableGames);
            var gameType = game.GameStateType;
            var gameState = ConstructType(gameType);

            typeof(Program).GetMethod(nameof(RunGame), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(game.MoveType).Invoke(null, new object[] { gameState });

            if (Debugger.IsAttached)
            {
                Console.ReadKey(intercept: true);
            }
        }

        private static void RunGame<TMove>(IGameState<TMove> state)
            where TMove : IMove
        {
            Console.WriteLine(Resources.GamePlayerCount, string.Format(state.Players.Count == 1 ? Resources.SingularPlayer : Resources.PluralPlayers, state.Players.Count));
            var catalog = new PlayerCatalog(Assembly.GetExecutingAssembly(), typeof(IGameState<>).Assembly);
            var players = catalog.FindPlayers(typeof(TMove));
            var consoleRenderer = ConsoleRenderer.Default<TMove>();

            IPlayer<TMove> choosePlayer(PlayerToken playerToken)
            {
                consoleRenderer.Show(state, FormatUtilities.ParseStringFormat(Resources.ChoosePlayer, playerToken));
                Console.WriteLine();
                var player = ConsoleInteraction.Choose(players);
                return (IPlayer<TMove>)ConstructType(player.PlayerType, p => p.Name == nameof(playerToken) && p.ParameterType == typeof(PlayerToken) ? playerToken : GetArgument(p));
            }

            IPlayer<TMove> getPlayer(PlayerToken playerToken)
            {
                var player = choosePlayer(playerToken);
                player.MessageSent += (obj, args) =>
                {
                    consoleRenderer.Show(state, FormatUtilities.ParseStringFormat(Resources.PlayerMessaged, playerToken));
                    consoleRenderer.Show(state, args.FormatTokens);
                    Console.WriteLine();
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
            consoleRenderer.Show(state, FormatUtilities.ParseStringFormat(Resources.PlayerMoved, move.PlayerToken));
            Console.WriteLine();

            consoleRenderer.Show(state, move);
            Console.WriteLine();

            Console.WriteLine();
            consoleRenderer.Show(state);
            Console.WriteLine();
        }
    }
}
