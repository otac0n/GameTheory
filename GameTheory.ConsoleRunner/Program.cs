// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Players;

    internal class Program
    {
        private static Type[] FindGames(Assembly assembly)
        {
            return (from t in assembly.ExportedTypes
                    where !t.IsAbstract
                    where GetMoveType(t) != null
                    select t).ToArray();
        }

        private static object GetArgument(ParameterInfo parameter)
        {
            Console.WriteLine($"{parameter.Name}:");

            if (parameter.ParameterType == typeof(int))
            {
                while (true)
                {
                    var line = Console.ReadLine();

                    int selection;
                    if (int.TryParse(line, out selection))
                    {
                        return selection;
                    }
                    else
                    {
                        Console.WriteLine("Selection must be a number.");
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static Type GetMoveType(Type gameStateType)
        {
            return (from i in gameStateType.GetInterfaces()
                    where i.IsConstructedGenericType
                    where i.GetGenericTypeDefinition() == typeof(IGameState<>)
                    select i.GetGenericArguments()[0]).FirstOrDefault();
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
            var assembly = typeof(IGameState<>).Assembly;
            var games = FindGames(assembly);
            var game = ConsoleInteraction.Choose(games);
            var constructor = ConsoleInteraction.Choose(game.GetConstructors(), skipMessage: _ => "Using only available constructor.");
            var args = constructor.GetParameters().Select(p => GetArgument(p)).ToArray();

            typeof(Program).GetMethod(nameof(RunGame), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(GetMoveType(game)).Invoke(null, new object[] { constructor.Invoke(args) });

            if (Debugger.IsAttached)
            {
                Console.ReadKey(intercept: true);
            }
        }

        private static void RunGame<TMove>(IGameState<TMove> gameState)
            where TMove : IMove
        {
            Console.WriteLine($"This game has {gameState.Players.Count} player{(gameState.Players.Count != 1 ? "s" : string.Empty)}");
            gameState = GameUtils.PlayGame(gameState, playerToken => GetPlayer(gameState, playerToken), ShowMove).Result;
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
