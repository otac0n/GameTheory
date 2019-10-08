// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using GameTheory.Gdl;
    using GameTheory.Gdl.Catalogs;
    using GameTheory.Gdl.Shared;
    using GameTheory.Players;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class GameCompilerTests
    {
        private interface IGameManager
        {
            object Run();
        }

        public static IEnumerable<string> Games =>
            Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(GameCompilerTests).Assembly.Location), "../../..")), "METADATA", SearchOption.AllDirectories);

        [TestCaseSource(nameof(Games))]
        public void Compile_WhenGivenAGameDefinition_ReturnsAFullyConstructedType(string game)
        {
            var gdl = LoadGameGdl(game, out var friendlyName);
            var result = new GameCompiler().Compile(gdl, friendlyName);

            GetDebugInfo(result, out var types, out var names, out var dependencies, out var code);

            Assert.That(result.Errors.Where(e => !e.IsWarning), Is.Empty);
            Assert.That(result.Type, Is.Not.Null);
        }

        [TestCaseSource(nameof(Games))]
        [Timeout(10000)]
        public void Compile_WhenGivenAGameDefinition_ReturnsAGameThatCanBePlayedToTheEnd(string game)
        {
            var gdl = LoadGameGdl(game, out var friendlyName);
            var result = new GameCompiler().Compile(gdl, friendlyName);

            GetDebugInfo(result, out var types, out var names, out var dependencies, out var code);

            Assume.That(result.Errors.Where(e => !e.IsWarning), Is.Empty);
            Assume.That(result.Type, Is.Not.Null);
            RunGame(result);
        }

        [Test]
        public void Compile_WhenGivenAGameWithAPotentialNamingConflict_ReturnsAGameThatCanBePlayedToTheEnd(
            [Values(
                @"(role x) (goal x 100) (<= (next ?x) (does x ?x)) (legal x {0}) (<= terminal (true {0}))",
                @"(role x) (goal x 100) (<= (next ?x) (does x ?x)) (legal x ({0})) (<= terminal (true ({0})))",
                @"(role x) (goal x 100) (<= (next ?x) (does x ?x)) (legal x ({0} 1)) (<= terminal (true ({0} 1)))",
                @"(role x) (goal x 100) (<= (next ?x) (does x ?x)) (legal x win) (<= terminal (true win)) {0}",
                @"(role x) (goal x 100) (<= (next ?x) (does x ?x)) (legal x (win {0})) (<= terminal (true (win {0})))",
                @"(role x) (goal x 100) (<= (next ?x) (does x ?x)) (legal x (win ?{0})) (<= terminal (true (win ?{0})))")]
            string game,
            [Values(
                "a",
                "Array",
                "b",
                "comp",
                "CompareTo",
                "Enum",
                "Equals",
                "FindForcedNoOps",
                "FormatTokens",
                "GameState",
                "GameTheory",
                "GetAvailableMoves",
                "GetOutcomes",
                "GetValues",
                "GetView",
                "GetWinners",
                "IXml",
                "List",
                "MakeMove",
                "maxStates",
                "Move",
                "moves",
                "object",
                "Players",
                "Role",
                "State",
                "System",
                "Task",
                "ToString",
                "ToXml",
                "Value",
                "var",
                "Weighted",
                "Where",
                "winners",
                "writer",
                "XmlWriter")]
            string conflict)
        {
            var result = new GameCompiler().Compile(string.Format(game, conflict));

            GetDebugInfo(result, out var types, out var names, out var dependencies, out var code);

            Assert.That(result.Errors.Where(e => !e.IsWarning), Is.Empty);
            Assert.That(result.Type, Is.Not.Null);
            RunGame(result);
        }

        [TestCase(@"(role a) terminal")]
        [TestCase(@"(role a) (goal a 100) terminal")]
        [TestCase(@"(role a) (goal a 100) (init win) (<= terminal (true win))")]
        [TestCase(@"(role a) (goal a 100) (<= (next ?x) (does a ?x)) terminal")]
        [TestCase(@"(role a) (goal a 100) (<= (next ?x) (does a ?x)) (legal a win) (<= terminal (true win))")]
        [TestCase(@"(role a) (init (cell 1 1 a))")]
        [TestCase(@"(role a) (init (cell 1 1 a)) (<= terminal (true (cell ?x ?y a)))")]
        [TestCase(@"(role a) (goal a 100) (legal a a) (<= (next ?x) (does a ?x)) terminal")]
        public void Compile_WhenGivenASimpleGame_ReturnsAGameThatCanBePlayedToTheEnd(string game)
        {
            var result = new GameCompiler().Compile(game);

            GetDebugInfo(result, out var types, out var names, out var dependencies, out var code);

            Assert.That(result.Errors.Where(e => !e.IsWarning), Is.Empty);
            Assert.That(result.Type, Is.Not.Null);
            RunGame(result);
        }

        [TestCase(@"(role a) (init (cell 1 1 a)) (legal a x)", "<state><fact><relation>cell</relation><argument>1</argument><argument>1</argument><argument>A</argument></fact></state>")]
        public async Task Compile_WhenGivenASimpleGame_ReturnsAGameThatCanBeSerializedToXml(string game, string xml)
        {
            var result = new GameCompiler().Compile(game);

            GetDebugInfo(result, out var types, out var names, out var dependencies, out var code);

            Assume.That(result.Errors.Where(e => !e.IsWarning), Is.Empty);
            Assume.That(result.Type, Is.Not.Null);
            var startingState = Activator.CreateInstance(result.Type);
            Assume.That(startingState, Is.InstanceOf<IXml>());

            var settings = new XmlWriterSettings
            {
                Async = true,
                OmitXmlDeclaration = true,
            };

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                await ((IXml)startingState).ToXml(writer);
            }

            Assert.That(sb.ToString(), Is.EqualTo(xml));
        }

        private static void GetDebugInfo(CompileResult result, out string types, out string names, out string dependencies, out string code)
        {
            types = DebuggingTools.RenderTypeGraph(result.AssignedTypes).Replace("\"", "\"\"");
            names = DebuggingTools.RenderNameGraph(result.KnowledgeBase, result.AssignedTypes).Replace("\"", "\"\"");
            dependencies = DebuggingTools.RenderDependencyGraph(result.DependencyGraph).Replace("\"", "\"\"");
            code = result.Code;
        }

        private static string LoadGameGdl(string path, out string friendlyName)
        {
            var metadata = JsonConvert.DeserializeObject<GameMetadata>(File.ReadAllText(path));
            var gdlPath = Path.Combine(Path.GetDirectoryName(path), metadata.RuleSheet);
            friendlyName = metadata.GameName;
            return File.ReadAllText(gdlPath);
        }

        private static void RunGame(CompileResult result)
        {
            var stateType = result.Type;
            var moveType = stateType.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGameState<>)).GetGenericArguments().Single();
            var startingState = Activator.CreateInstance(stateType);
            var manager = (IGameManager)Activator.CreateInstance(typeof(GameManager<>).MakeGenericType(moveType), startingState);
            var endState = manager.Run();
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
