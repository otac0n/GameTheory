// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Gdl;
    using GameTheory.Gdl.Types;
    using GameTheory.Players;
    using NUnit.Framework;

    [TestFixture]
    public class GameCompilerTests
    {
        public static IEnumerable<string> Games =>
            from r in Assembly.GetExecutingAssembly().GetManifestResourceNames()
            where r.EndsWith(".gdl", StringComparison.InvariantCultureIgnoreCase) || r.EndsWith(".kif", StringComparison.InvariantCultureIgnoreCase)
            select r;

        [TestCaseSource(nameof(Games))]
        public void Compile_WhenGivenAGameDefinition_ReturnsAFullyConstructedType(string game)
        {
            var gdl = LoadAssemblyResource(game);
            var compiler = new GameCompiler();
            var result = compiler.Compile(gdl);

            var graph = DebuggingTools.RenderTypeGraph(result.ExpressionTypes.Values.Where(v => !(v is ObjectInfo objectInfo && objectInfo.Value is int))).Replace("\"", "\"\"");

            Assert.IsEmpty(result.Errors.Where(e => !e.IsWarning));
            Assert.NotNull(result.Type);
        }

        [TestCaseSource(nameof(Games))]
        public void Compile_WhenGivenAGameDefinition_ReturnsAGameThatCanBePlayedToTheEnd(string game)
        {
            var gdl = LoadAssemblyResource(game);
            var compiler = new GameCompiler();
            var result = compiler.Compile(gdl);

            var graph = DebuggingTools.RenderTypeGraph(result.ExpressionTypes.Values.Where(v => !(v is ObjectInfo objectInfo && objectInfo.Value is int))).Replace("\"", "\"\"");

            var stateType = result.Type;
            var moveType = stateType.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGameState<>)).GetGenericArguments().Single();
            var manager = (IGameManager)Activator.CreateInstance(typeof(GameManager<>).MakeGenericType(moveType), Activator.CreateInstance(stateType));
            var endState = manager.Run();
        }

        private static string LoadAssemblyResource(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private interface IGameManager
        {
            object Run();
        }

        private class GameManager<TMove> : IGameManager
            where TMove : IMove
        {
            private IGameState<TMove> state;

            public GameManager(IGameState<TMove> state)
            {
                this.state = state;
            }

            public object Run()
            {
                return GameUtilities.PlayGame(
                    this.state,
                    p => new RandomPlayer<TMove>(p),
                    (prevState, move, state) => Console.WriteLine("{0}: {1}", state.GetPlayerName(move.PlayerToken), move)).Result;
            }
        }
    }
}
