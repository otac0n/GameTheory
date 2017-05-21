// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Catalogs;
    using GameTheory.Players;

    internal class Program
    {
        private static object GetArgument(ParameterInfo parameter)
        {
            Console.WriteLine($"{parameter.Name}:");

            if (parameter.ParameterType == typeof(int))
            {
                while (true)
                {
                    var line = Console.ReadLine();
                    if (int.TryParse(line, out int selection))
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

        private static IPlayer<TMove> GetPlayer<TMove>(IGameState<TMove> gameState, PlayerToken playerToken)
            where TMove : IMove
        {
            Console.WriteLine($"Choose {gameState.GetPlayerName(playerToken)}:");
            var playerType = ConsoleInteraction.Choose(new[] { typeof(RandomPlayer<TMove>), typeof(ConsolePlayer<TMove>) });
            return (IPlayer<TMove>)Activator.CreateInstance(playerType, new[] { playerToken });
        }

        private static void Main()
        {
            var catalog = GameCatalog.Default;
            var game = ConsoleInteraction.Choose(catalog.AvailableGames);
            var constructor = ConsoleInteraction.Choose(game.GameStateType.GetConstructors(), skipMessage: _ => "Using only available constructor.");
            var args = constructor.GetParameters().Select(p => GetArgument(p)).ToArray();

            typeof(Program).GetMethod(nameof(RunGame), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(game.MoveType).Invoke(null, new object[] { constructor.Invoke(args) });

            if (Debugger.IsAttached)
            {
                Console.ReadKey(intercept: true);
            }
        }

        private static void RunGame<TMove>(IGameState<TMove> gameState)
            where TMove : IMove
        {
            Console.WriteLine($"This game has {gameState.Players.Count} player{(gameState.Players.Count != 1 ? "s" : string.Empty)}");
            gameState = GameUtilities.PlayGame(gameState, playerToken => GetPlayer(gameState, playerToken), ShowMove).Result;
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
