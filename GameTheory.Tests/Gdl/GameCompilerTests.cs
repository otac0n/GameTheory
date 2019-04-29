// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Gdl;
    using GameTheory.Players;
    using NUnit.Framework;

    [TestFixture]
    public class GameCompilerTests
    {
        public static IEnumerable<string> Games =>
            from r in Assembly.GetExecutingAssembly().GetManifestResourceNames()
            where r.EndsWith(".gdl", StringComparison.InvariantCultureIgnoreCase) || r.EndsWith(".kif", StringComparison.InvariantCultureIgnoreCase)
            select r;

        [TestCase(@"(role a) (goal a 100) terminal")]
        [TestCase(@"(role a) (goal a 100) (init win) (<= terminal (true win))")]
        [TestCase(@"(role a) (goal a 100) (<= (next ?x) (does a ?x)) (legal a win) (<= terminal (true win))")]
        public void Compile_WhenGivenASimpleGame_ReturnsAGameThatCanBePlayedToTheEnd(string game)
        {
            var result = new GameCompiler().Compile(game);

            GetDebugInfo(result, out var types, out var names, out var dependencies, out var code);

            Assert.That(result.Errors.Where(e => !e.IsWarning), Is.Empty);
            Assert.That(result.Type, Is.Not.Null);
            RunGame(result);
        }

        [TestCaseSource(nameof(Games))]
        public void Compile_WhenGivenAGameDefinition_ReturnsAFullyConstructedType(string game)
        {
            var gdl = LoadAssemblyResource(game, out var friendlyName);
            var result = new GameCompiler().Compile(gdl, friendlyName);

            GetDebugInfo(result, out var types, out var names, out var dependencies, out var code);

            Assert.That(result.Errors.Where(e => !e.IsWarning), Is.Empty);
            Assert.That(result.Type, Is.Not.Null);
        }

        [TestCaseSource(nameof(Games))]
        [Timeout(120000)]
        public void Compile_WhenGivenAGameDefinition_ReturnsAGameThatCanBePlayedToTheEnd(string game)
        {
            var gdl = LoadAssemblyResource(game, out var friendlyName);
            var result = new GameCompiler().Compile(gdl, friendlyName);

            GetDebugInfo(result, out var types, out var names, out var dependencies, out var code);

            Assume.That(result.Errors.Where(e => !e.IsWarning), Is.Empty);
            Assume.That(result.Type, Is.Not.Null);
            RunGame(result);
        }

        private static void RunGame(CompileResult result)
        {
            var stateType = result.Type;
            var moveType = stateType.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGameState<>)).GetGenericArguments().Single();
            var startingState = Activator.CreateInstance(stateType);
            var manager = (IGameManager)Activator.CreateInstance(typeof(GameManager<>).MakeGenericType(moveType), startingState);
            var endState = manager.Run();
        }

        private static string LoadAssemblyResource(string name, out string friendlyName)
        {
            var prefix = typeof(GameCompilerTests).Namespace + ".Games.";
            friendlyName = name.Replace(prefix, prefix.Replace('.', '\\'));

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static void GetDebugInfo(CompileResult result, out string types, out string names, out string dependencies, out string code)
        {
            types = DebuggingTools.RenderTypeGraph(result.AssignedTypes).Replace("\"", "\"\"");
            names = DebuggingTools.RenderNameGraph(result.KnowledgeBase, result.AssignedTypes).Replace("\"", "\"\"");
            dependencies = DebuggingTools.RenderDependencyGraph(result.DependencyGraph).Replace("\"", "\"\"");
            code = result.Code;
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
