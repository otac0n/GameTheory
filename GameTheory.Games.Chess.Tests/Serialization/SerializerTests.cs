// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Tests.Serialization
{
    using System.IO;
    using GameTheory.Games.Chess.Serialization;
    using NUnit.Framework;

    public class SerializerTests
    {
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
        [TestCase("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1")]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2")]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2")]
        [TestCase("8/8/8/8/8/8/8/8 w - - 0 1")]
        public void SerializeFen_WhenGivenValidState_RoundTripsFen(string subject)
        {
            var state = new GameState(subject);
            var serialized = Serializer.SerializeFen(state);
            Assert.That(serialized, Is.EqualTo(subject));
        }

        [Test]
        public void SerializePgn_WhenGivenValidFile_RoundTripsPgnInTwoSteps([ValueSource(typeof(PgnParserTests), nameof(PgnParserTests.ValidPgnKeys))] string key)
        {
            var input = PgnParserTests.ValidPgnFiles[key];
            var parser = new PgnParser();

            var round1 = parser.Parse(input);
            var serializer1 = new StringWriter();
            Serializer.SerializePgn(serializer1, round1);
            var output1 = serializer1.ToString();

            var round2 = parser.Parse(output1);
            var serializer2 = new StringWriter();
            Serializer.SerializePgn(serializer2, round2);
            var output2 = serializer2.ToString();

            Assert.That(output2, Is.EqualTo(output1));
        }
    }
}
