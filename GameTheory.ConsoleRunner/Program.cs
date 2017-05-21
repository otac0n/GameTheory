// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Catalogs;

    internal class Program
    {
        private static object GetArgument(ParameterInfo parameter)
        {
            Console.Write($"{parameter.Name}:");

            if (parameter.HasDefaultValue)
            {
                Console.Write($" [default {parameter.DefaultValue}]");
            }

            Console.WriteLine();

            if (parameter.ParameterType == typeof(int))
            {
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
                            Console.WriteLine("No default available.");
                        }
                    }
                    else if (int.TryParse(line, out int selection))
                    {
                        return selection;
                    }
                    else
                    {
                        Console.WriteLine("Selection must be a number.");
                    }
                }
            }
            else if (parameter.ParameterType == typeof(bool))
            {
                while (true)
                {
                    var line = Console.ReadLine();
                    switch (line.ToUpperInvariant())
                    {
                        case "":
                            if (parameter.HasDefaultValue)
                            {
                                return parameter.DefaultValue;
                            }
                            else
                            {
                                Console.WriteLine("No default available.");
                                break;
                            }

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
                            Console.WriteLine("Selection must be a boolean value.");
                            break;
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static IPlayer<TMove> GetPlayer<TMove>(IList<Player> players, IGameState<TMove> gameState, PlayerToken playerToken)
            where TMove : IMove
        {
            Console.WriteLine($"Choose {gameState.GetPlayerName(playerToken)}:");
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
            var constructor = ConsoleInteraction.Choose(type.GetConstructors(), skipMessage: _ => "Using only available constructor.");
            var args = constructor.GetParameters().Select(getParameter).ToArray();
            return constructor.Invoke(args);
        }

        private static void RunGame<TMove>(IGameState<TMove> gameState)
            where TMove : IMove
        {
            Console.WriteLine($"This game has {gameState.Players.Count} player{(gameState.Players.Count != 1 ? "s" : string.Empty)}");
            var catalog = new PlayerCatalog(typeof(IGameState<>).Assembly, Assembly.GetExecutingAssembly());
            var players = catalog.FindPlayers(typeof(TMove));
            gameState = GameUtilities.PlayGame(gameState, playerToken => GetPlayer(players, gameState, playerToken), ShowMove).Result;
            Console.WriteLine($"Final state:");
            Console.WriteLine(gameState);

            Console.WriteLine($"Winners:");
            var anyWinners = false;
            foreach (var winner in gameState.GetWinners())
            {
                anyWinners = true;
                Console.WriteLine(gameState.GetPlayerName(winner));
            }

            if (!anyWinners)
            {
                Console.WriteLine("(none)");
            }
        }

        private static void ShowMove<TMove>(IGameState<TMove> gameState, TMove move)
            where TMove : IMove
        {
            Console.WriteLine($"{gameState.GetPlayerName(move.PlayerToken)} moved:");
            Console.WriteLine(move);
        }
    }
}
