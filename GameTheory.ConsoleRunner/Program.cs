// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Catalogs;
    using GameTheory.ConsoleRunner.ConsoleRenderers;
    using GameTheory.ConsoleRunner.Properties;

    internal class Program
    {
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

        private static IPlayer<TMove> GetPlayer<TMove>(IList<Player> players, IGameState<TMove> gameState, PlayerToken playerToken)
            where TMove : IMove
        {
            Console.WriteLine(Resources.ChoosePlayer, gameState.GetPlayerName(playerToken));
            var player = ConsoleInteraction.Choose(players);
            return (IPlayer<TMove>)ConstructType(player.PlayerType, p => p.Name == nameof(playerToken) && p.ParameterType == typeof(PlayerToken) ? playerToken : GetArgument(p));
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

        private static object ConstructType(Type type, Func<ParameterInfo, object> getParameter = null)
        {
            getParameter = getParameter ?? (p => GetArgument(p));
            var constructor = ConsoleInteraction.Choose(type.GetConstructors(), skipMessage: _ => Resources.SingleConstructor);
            var args = constructor.GetParameters().Select(getParameter).ToArray();
            return constructor.Invoke(args);
        }

        private static void RunGame<TMove>(IGameState<TMove> gameState)
            where TMove : IMove
        {
            Console.WriteLine(Resources.GamePlayerCount, string.Format(gameState.Players.Count == 1 ? Resources.SingularPlayer : Resources.PluralPlayers, gameState.Players.Count));
            var catalog = new PlayerCatalog(Assembly.GetExecutingAssembly(), typeof(IGameState<>).Assembly);
            var players = catalog.FindPlayers(typeof(TMove));
            var consoleRenderer = ConsoleRenderer.Default<TMove>();

            IPlayer<TMove> getPlayer(PlayerToken playerToken)
            {
                var player = GetPlayer(players, gameState, playerToken);
                player.MessageSent += (obj, args) =>
                {
                    Console.Write(gameState.GetPlayerName(playerToken));
                    Console.Write(" Messaged: ");
                    Console.WriteLine(args.Message);
                };
                return player;
            }

            gameState = GameUtilities.PlayGame(gameState, getPlayer, (prevState, move, state) => ShowMove(state, move, consoleRenderer), TimeSpan.FromMinutes(5)).Result;
            Console.WriteLine(Resources.FinalState);
            consoleRenderer.Show(gameState);

            Console.WriteLine(Resources.Winners);
            var anyWinners = false;
            foreach (var winner in gameState.GetWinners())
            {
                anyWinners = true;
                Console.WriteLine(gameState.GetPlayerName(winner));
            }

            if (!anyWinners)
            {
                Console.WriteLine(Resources.None);
            }
        }

        private static void ShowMove<TMove>(IGameState<TMove> gameState, TMove move, IConsoleRenderer<TMove> consoleRenderer)
            where TMove : IMove
        {
            Console.WriteLine(Resources.PlayerMoved, gameState.GetPlayerName(move.PlayerToken));
            Console.WriteLine(move);
            consoleRenderer.Show(gameState);
        }
    }
}
