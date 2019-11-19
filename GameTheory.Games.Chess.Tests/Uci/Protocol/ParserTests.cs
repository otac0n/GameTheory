// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Tests.Uci.Protocol
{
    using GameTheory.Games.Chess.Uci.Protocol;
    using NUnit.Framework;

    [TestFixture]
    public class ParserTests
    {
        [TestCase("", true)]
        [TestCase("ok", true)]
        [TestCase("uci", false)]
        [TestCase("debug on", false)]
        [TestCase("debug off", false)]
        [TestCase("isready", false)]
        [TestCase("setoption name Clear Hash", false)]
        [TestCase("setoption name Selectivity value 3", false)]
        [TestCase("setoption name UCI_Chess960 value true", false)]
        [TestCase("register later", false)]
        [TestCase("register name OK Name code okcode", false)]
        [TestCase("ucinewgame", false)]
        [TestCase("position startpos", false)]
        [TestCase("position startpos moves e2e4 e7e5", false)]
        [TestCase("position fen rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1", false)]
        [TestCase("position fen rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1 moves e7e5", false)]
        [TestCase("position moves e7e5", false)]
        [TestCase("go", false)]
        [TestCase("go infinite", false)]
        [TestCase("go depth 5", false)]
        [TestCase("go nodes 1000", false)]
        [TestCase("go mate 3", false)]
        [TestCase("go wtime 5000 btime 5000", false)]
        [TestCase("go wtime 5000 btime 5000 movestogo 4", false)]
        [TestCase("go infinite searchmoves e2e4 d2d4", false)]
        [TestCase("stop", false)]
        [TestCase("ponderhit", false)]
        [TestCase("quit", false)]
        [TestCase("id name OK Name", false)]
        [TestCase("id author OK Author", false)]
        [TestCase("uciok", false)]
        [TestCase("readyok", false)]
        [TestCase("bestmove e2e4", false)]
        [TestCase("bestmove e2e4 ponder e7e5", false)]
        [TestCase("copyprotection checking", false)]
        [TestCase("copyprotection error", false)]
        [TestCase("copyprotection ok", false)]
        [TestCase("registration checking", false)]
        [TestCase("registration error", false)]
        [TestCase("registration ok", false)]
        [TestCase("info currmove e2e4 currmovenumber 1", false)]
        [TestCase("info depth 12 nodes 123456 nps 100000", false)]
        [TestCase("info depth 2 score cp 214 time 1242 nodes 2124 nps 34928 pv e2e4 e7e5 g1f3", false)]
        [TestCase("option name Nullmove type check default true", false)]
        [TestCase("option name Selectivity type spin default 2 min 0 max 4", false)]
        [TestCase("option name Style type combo default Normal var Solid var Normal var Risky", false)]
        [TestCase("option name NalimovPath type string default <empty>", false)]
        [TestCase("option name NalimovPath type string default ", false)]
        [TestCase("option name Clear Hash type button", false)]
        public void Parse_WithAnyCommandText_RoundTripsTheText(string subject, bool isUnknown)
        {
            var parser = new Parser();
            var result = parser.Parse(subject);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.AssignableTo<Command>());
            Assert.That(result, isUnknown ? Is.InstanceOf<UnknownCommand>() : Is.Not.InstanceOf<UnknownCommand>());
            Assert.That(result.ToString(), Is.EqualTo(subject));
        }
    }
}
