// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
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

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            NativeMethods.SetConsoleFont();
            NativeMethods.Maximize();

            var game = ConsoleInteraction.Choose(GameCatalog.AvailableGames);
            var gameType = game.GameStateType;
            var state = ConsoleInteraction.ConstructType(game.Initializers);

            var genericMethod = typeof(Program).GetMethod(nameof(RunGame), BindingFlags.Static | BindingFlags.NonPublic);
            var constructedMethod = genericMethod.MakeGenericMethod(gameType, game.MoveType);
            constructedMethod.Invoke(null, new object[] { state });

            if (Debugger.IsAttached)
            {
                Console.ReadKey(intercept: true);
            }
        }

        private static void RunGame<TGameState, TMove>(TGameState state)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            Console.WriteLine(Resources.GamePlayerCount, string.Format(state.Players.Count == 1 ? Resources.SingularPlayer : Resources.PluralPlayers, state.Players.Count));
            var players = PlayerCatalog.FindPlayers(typeof(TGameState), typeof(TMove));
            var rendererType = ConsoleRendererCatalog
                .FindConsoleRenderers<TGameState, TMove>()
                .OrderBy(t => t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(ToStringConsoleRenderer<,>))
                .First();
            var consoleRenderer = (IConsoleRenderer<TGameState, TMove>)Activator.CreateInstance(rendererType);
            var font = consoleRenderer.GetType().GetCustomAttributes(inherit: true).OfType<ConsoleFontAttribute>().FirstOrDefault();
            if (font != null)
            {
                NativeMethods.Restore();
                NativeMethods.SetConsoleFont(font.Name, font.XSize, font.YSize);
                NativeMethods.Maximize();
            }

            Console.WriteLine(Resources.StartingState);
            consoleRenderer.Show(state);

            IPlayer<TGameState, TMove> ChoosePlayer(PlayerToken playerToken)
            {
                consoleRenderer.Show(state, FormatUtilities.ParseStringFormat(Resources.ChoosePlayer, playerToken));
                Console.WriteLine();
                var player = ConsoleInteraction.Choose(players as IList<ICatalogPlayer> ?? players.ToList(), render: p => Console.Write(p.Name));
                return (IPlayer<TGameState, TMove>)ConsoleInteraction.ConstructType(player.Initializers, p =>
                    p.Name == nameof(playerToken) && p.ParameterType == typeof(PlayerToken) ? playerToken :
                    typeof(IConsoleRenderer<TGameState, TMove>).IsAssignableFrom(p.ParameterType) ? consoleRenderer :
                    ConsoleInteraction.GetArgument(p));
            }

            IPlayer<TGameState, TMove> GetPlayer(PlayerToken playerToken)
            {
                var player = ChoosePlayer(playerToken);
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

            state = GameUtilities.PlayGame(state, GetPlayer, (prevState, move, newState) => ShowMove(newState, move, consoleRenderer), TimeSpan.FromMinutes(5)).Result;
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

        private static void ShowMove<TGameState, TMove>(TGameState state, TMove move, IConsoleRenderer<TGameState, TMove> consoleRenderer)
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
