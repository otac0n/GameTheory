// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Games.Chess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.Chess;
    using NUnit.Framework;

    [TestFixture]
    public class ParserTests
    {
        [TestCase("", 0)]
        [TestCase("//", 1)]
        [TestCase("zz26", 0)]
        public void TryParseCoordinate_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseCoordinate(subject, ref index, out var actual);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(actual, Is.EqualTo(default(Point)));
        }

        [TestCase("e4", "4,3")]
        [TestCase("a1", "0,0")]
        [TestCase("h8", "7,7")]
        [TestCase("z2147483647", "25,2147483646")]
        public void TryParseCoordinate_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, string expectedStr)
        {
            var expectedParts = expectedStr.Split(',');
            var expected = new Point(int.Parse(expectedParts[0]), int.Parse(expectedParts[1]));

            var index = 0;
            var result = Parser.TryParseCoordinate(subject, ref index, out var actual);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(subject.Length));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("", 0)]
        [TestCase("--", 1)]
        [TestCase("101 w - - 0 1", 0)]
        [TestCase("1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1 W - - 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 @", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w e4 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR KQkq e4 0 1", 0)]
        public void TryParseFen_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFen(subject, ref index, out var board, out var activePlayer, out var castling, out var epCoordinate, out var plyCountClock, out var moveNumber);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(board, Is.EqualTo(default(Pieces[,])));
            Assert.That(activePlayer, Is.EqualTo(default(Pieces)));
            Assert.That(castling, Is.EqualTo(default(HashSet<Pieces>)));
            Assert.That(epCoordinate, Is.EqualTo(default(Point?)));
            Assert.That(plyCountClock, Is.EqualTo(default(int)));
            Assert.That(moveNumber, Is.EqualTo(default(int)));
        }

        [TestCase("100 w - - 0 1", Pieces.White, null, null, null, 0, 1)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", Pieces.White, "White,King;White,Queen;Black,King;Black,Queen", null, null, 0, 1)]
        [TestCase("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", Pieces.Black, "White,King;White,Queen;Black,King;Black,Queen", 4, 2, 0, 1)]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2", Pieces.White, "White,King;White,Queen;Black,King;Black,Queen", 2, 5, 0, 2)]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2", Pieces.Black, "White,King;White,Queen;Black,King;Black,Queen", null, null, 1, 2)]
        [TestCase("8/8/8/8/8/8/8/8 w - - 0 1", Pieces.White, null, null, null, 0, 1)]
        public void TryParseFen_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, Pieces expectedActivePlayer, string expectedCastlingStr, int? expectedEpX, int? expectedEpY, int expectedPlyCountClock, int expectedMoveNumber)
        {
            var expectedCastling = expectedCastlingStr == null
                ? null
                : expectedCastlingStr.Split(';').Select(s => (Pieces)Enum.Parse(typeof(Pieces), s)).ToList();
            var expectedEpCoordinate = expectedEpX == null || expectedEpY == null
                ? default(Point?)
                : new Point(expectedEpX.Value, expectedEpY.Value);

            var index = 0;
            var result = Parser.TryParseFen(subject, ref index, out var board, out var activePlayer, out var castling, out var epCoordinate, out var plyCountClock, out var moveNumber);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(subject.Length));
            Assert.That(activePlayer, Is.EqualTo(expectedActivePlayer));

            if (expectedCastling == null)
            {
                Assert.That(castling, Is.Null);
            }
            else
            {
                Assert.That(castling, Is.EquivalentTo(expectedCastling));
            }

            Assert.That(epCoordinate, Is.EqualTo(expectedEpCoordinate));
            Assert.That(plyCountClock, Is.EqualTo(expectedPlyCountClock));
            Assert.That(moveNumber, Is.EqualTo(expectedMoveNumber));
        }

        [TestCase("", 0)]
        [TestCase("--", 1)]
        [TestCase("101", 0)]
        [TestCase("100p", 0)]
        [TestCase("pppppppp/8/7/8", 0)]
        public void TryParseFenBoard_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFenBoard(subject, ref index, out var actual);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(actual, Is.EqualTo(default(List<Pieces>)));
        }

        [TestCase("10", 2, "None;None;None;None;None;None;None;None;None;None")]
        [TestCase("1/-", 1, "None")]
        [TestCase("8/8/8/8/8/8/8/8", 15, "None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None")]
        [TestCase("1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1", 53, "None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None")]
        [TestCase("27", 2, "None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None;None")]
        public void TryParseFenBoard_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, int expectedIndex, string expectedStr)
        {
            var expected = expectedStr.Split(';').Select(s => (Pieces)Enum.Parse(typeof(Pieces), s)).ToList();

            var index = 0;
            var result = Parser.TryParseFenBoard(subject, ref index, out var actual);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(expectedIndex));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("", 0)]
        [TestCase("//", 1)]
        public void TryParseFenCastling_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFenCastling(subject, ref index, out var actual);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(actual, Is.EqualTo(default(HashSet<Pieces>)));
        }

        [TestCase("K", "White,King")]
        [TestCase("Q", "White,Queen")]
        [TestCase("k", "Black,King")]
        [TestCase("q", "Black,Queen")]
        [TestCase("KQkq", "White,King;White,Queen;Black,King;Black,Queen")]
        public void TryParseFenCastling_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, string expectedStr)
        {
            var expected = expectedStr.Split(';').Select(s => (Pieces)Enum.Parse(typeof(Pieces), s)).ToList();

            var index = 0;
            var result = Parser.TryParseFenCastling(subject, ref index, out var actual);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(subject.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [TestCase("", 0)]
        [TestCase("//", 1)]
        public void TryParseFenColor_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFenColor(subject, ref index, out var actual);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(actual, Is.EqualTo(default(Pieces)));
        }

        [TestCase("b", 0, Pieces.Black)]
        [TestCase("w", 0, Pieces.White)]
        public void TryParseFenColor_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, int index, Pieces expected)
        {
            var startIndex = index;
            var result = Parser.TryParseFenColor(subject, ref index, out var actual);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(startIndex + 1));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("", 0)]
        [TestCase("//", 1)]
        public void TryParseFenPiece_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFenPiece(subject, ref index, out var actual);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(actual, Is.EqualTo(default(Pieces)));
        }

        [TestCase("p", 0, Pieces.Black | Pieces.Pawn)]
        [TestCase("n", 0, Pieces.Black | Pieces.Knight)]
        [TestCase("b", 0, Pieces.Black | Pieces.Bishop)]
        [TestCase("r", 0, Pieces.Black | Pieces.Rook)]
        [TestCase("q", 0, Pieces.Black | Pieces.Queen)]
        [TestCase("k", 0, Pieces.Black | Pieces.King)]
        [TestCase("P", 0, Pieces.White | Pieces.Pawn)]
        [TestCase("N", 0, Pieces.White | Pieces.Knight)]
        [TestCase("B", 0, Pieces.White | Pieces.Bishop)]
        [TestCase("R", 0, Pieces.White | Pieces.Rook)]
        [TestCase("Q", 0, Pieces.White | Pieces.Queen)]
        [TestCase("K", 0, Pieces.White | Pieces.King)]
        [TestCase("/P/", 1, Pieces.White | Pieces.Pawn)]
        public void TryParseFenPiece_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, int index, Pieces expected)
        {
            var startIndex = index;
            var result = Parser.TryParseFenPiece(subject, ref index, out var actual);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(startIndex + 1));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("", 0)]
        [TestCase("//", 1)]
        [TestCase("101", 0)]
        [TestCase("100p", 0)]
        public void TryParseFenRank_WhenGivenInvalidInput_ReturnsTheExpectedValue(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFenRank(subject, ref index, out var actual);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(actual, Is.EqualTo(default(List<Pieces>)));
        }

        [TestCase("8", "None;None;None;None;None;None;None;None")]
        [TestCase("1p1p1p1p", "None;Black,Pawn;None;Black,Pawn;None;Black,Pawn;None;Black,Pawn")]
        [TestCase("P1P1P1P1", "White,Pawn;None;White,Pawn;None;White,Pawn;None;White,Pawn;None")]
        [TestCase("rnbqk", "Black,Rook;Black,Knight;Black,Bishop;Black,Queen;Black,King")]
        [TestCase("QKBNR", "White,Queen;White,King;White,Bishop;White,Knight;White,Rook")]
        [TestCase("10", "None;None;None;None;None;None;None;None;None;None")]
        public void TryParseFenRank_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, string expectedStr)
        {
            var expected = expectedStr.Split(';').Select(s => (Pieces)Enum.Parse(typeof(Pieces), s)).ToList();

            var index = 0;
            var result = Parser.TryParseFenRank(subject, ref index, out var actual);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(subject.Length));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("", 0)]
        [TestCase("p6p1p4p1", 3)]
        public void TryParseFenRankSeparator_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFenRankSeparator(subject, ref index);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
        }

        [TestCase("/", 0)]
        [TestCase("p6p/1p4p1", 3)]
        public void TryParseFenRankSeparator_WhenGivenValidInput_ReturnsTrue(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFenRankSeparator(subject, ref index);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(startIndex + 1));
        }

        [TestCase("", 0)]
        [TestCase("abc123", 3)]
        public void TryParseFenRecordSeparator_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFenRecordSeparator(subject, ref index);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
        }

        [TestCase(" ", 0)]
        [TestCase("abc 123", 3)]
        public void TryParseFenRecordSeparator_WhenGivenValidInput_ReturnsTrue(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseFenRecordSeparator(subject, ref index);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(startIndex + 1));
        }

        [TestCase("", 0)]
        [TestCase("1", 1)]
        [TestCase("A", 0)]
        [TestCase("2147483648", 0)]
        public void TryParseInt32_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseInt32(subject, ref index, out var actual);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(actual, Is.EqualTo(default(int)));
        }

        [TestCase("1", 0, 1)]
        [TestCase("10", 0, 10)]
        [TestCase("2147483647", 0, int.MaxValue)]
        [TestCase("/37/", 1, 37)]
        public void TryParseInt32_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, int index, int expected)
        {
            var startIndex = index;
            var result = Parser.TryParseInt32(subject, ref index, out var actual);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(startIndex + expected.ToString().Length));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("", 0)]
        [TestCase("--", 1)]
        [TestCase("27 w - - 0 1", 0)]
        [TestCase("1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1 W - - 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah - 0 @", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah - 0 ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah - 0", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah - ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah -", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w e4 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR AHah e4 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w Jj - 0 1", 0)]
        [TestCase("7/7/7/7/7/7/7/7 w AHah - 0 1", 0)]
        public void TryParseShredderFen_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseShredderFen(subject, ref index, out var board, out var activePlayer, out var castling, out var epCoordinate, out var plyCountClock, out var moveNumber);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(board, Is.EqualTo(default(Pieces[,])));
            Assert.That(activePlayer, Is.EqualTo(default(Pieces)));
            Assert.That(castling, Is.EqualTo(default(HashSet<Pieces>)));
            Assert.That(epCoordinate, Is.EqualTo(default(Point?)));
            Assert.That(plyCountClock, Is.EqualTo(default(int)));
            Assert.That(moveNumber, Is.EqualTo(default(int)));
        }

        [TestCase("26 w - - 0 1", Pieces.White, null, null, null, 0, 1)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah - 0 1", Pieces.White, "White:0;White:7;Black:0;Black:7", null, null, 0, 1)]
        [TestCase("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b AHah e3 0 1", Pieces.Black, "White:0;White:7;Black:0;Black:7", 4, 2, 0, 1)]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w AHah c6 0 2", Pieces.White, "White:0;White:7;Black:0;Black:7", 2, 5, 0, 2)]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b AHah - 1 2", Pieces.Black, "White:0;White:7;Black:0;Black:7", null, null, 1, 2)]
        [TestCase("8/8/8/8/8/8/8/8 w - - 0 1", Pieces.White, null, null, null, 0, 1)]
        public void TryParseShredderFen_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, Pieces expectedActivePlayer, string expectedCastlingStr, int? expectedEpX, int? expectedEpY, int expectedPlyCountClock, int expectedMoveNumber)
        {
            var expectedCastling = expectedCastlingStr == null
                ? null
                : expectedCastlingStr.Split(';').Select(s =>
                {
                    var parts = s.Split(':');
                    return Tuple.Create((Pieces)Enum.Parse(typeof(Pieces), parts[0]), int.Parse(parts[1]));
                }).ToList();
            var expectedEpCoordinate = expectedEpX == null || expectedEpY == null
                ? default(Point?)
                : new Point(expectedEpX.Value, expectedEpY.Value);

            var index = 0;
            var result = Parser.TryParseShredderFen(subject, ref index, out var board, out var activePlayer, out var castling, out var epCoordinate, out var plyCountClock, out var moveNumber);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(subject.Length));
            Assert.That(activePlayer, Is.EqualTo(expectedActivePlayer));

            if (expectedCastling == null)
            {
                Assert.That(castling, Is.Null);
            }
            else
            {
                Assert.That(castling, Is.EquivalentTo(expectedCastling));
            }

            Assert.That(epCoordinate, Is.EqualTo(expectedEpCoordinate));
            Assert.That(plyCountClock, Is.EqualTo(expectedPlyCountClock));
            Assert.That(moveNumber, Is.EqualTo(expectedMoveNumber));
        }

        [TestCase("", 0)]
        [TestCase("//", 1)]
        public void TryParseShredderFenCastling_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseShredderFenCastling(subject, ref index, out var actual);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(actual, Is.EqualTo(default(HashSet<Tuple<Pieces, int>>)));
        }

        [TestCase("A", "White:0")]
        [TestCase("B", "White:1")]
        [TestCase("C", "White:2")]
        [TestCase("D", "White:3")]
        [TestCase("E", "White:4")]
        [TestCase("F", "White:5")]
        [TestCase("G", "White:6")]
        [TestCase("H", "White:7")]
        [TestCase("J", "White:9")]
        [TestCase("Z", "White:25")]
        [TestCase("a", "Black:0")]
        [TestCase("b", "Black:1")]
        [TestCase("c", "Black:2")]
        [TestCase("d", "Black:3")]
        [TestCase("e", "Black:4")]
        [TestCase("f", "Black:5")]
        [TestCase("g", "Black:6")]
        [TestCase("h", "Black:7")]
        [TestCase("j", "Black:9")]
        [TestCase("z", "Black:25")]
        [TestCase("AHah", "White:0;White:7;Black:0;Black:7")]
        public void TryParseShredderFenCastling_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, string expectedStr)
        {
            var expected = expectedStr.Split(';').Select(s =>
            {
                var parts = s.Split(':');
                return Tuple.Create((Pieces)Enum.Parse(typeof(Pieces), parts[0]), int.Parse(parts[1]));
            }).ToList();

            var index = 0;
            var result = Parser.TryParseShredderFenCastling(subject, ref index, out var actual);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(subject.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [TestCase("", 0)]
        [TestCase("--", 1)]
        [TestCase("11 w - - 0 1", 0)]
        [TestCase("1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1/1 W - - 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 @", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR ", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w e4 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR KQkq e4 0 1", 0)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w Jj - 0 1", 0)]
        [TestCase("7/7/7/7/7/7/7/7 w AHah - 0 1", 0)]
        public void TryParseXFen_WhenGivenInvalidInput_ReturnsFalse(string subject, int index)
        {
            var startIndex = index;
            var result = Parser.TryParseXFen(subject, ref index, out var board, out var activePlayer, out var castling, out var epCoordinate, out var plyCountClock, out var moveNumber);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(board, Is.EqualTo(default(Pieces[,])));
            Assert.That(activePlayer, Is.EqualTo(default(Pieces)));
            Assert.That(castling, Is.EqualTo(default(HashSet<Tuple<Pieces, int>>)));
            Assert.That(epCoordinate, Is.EqualTo(default(Point?)));
            Assert.That(plyCountClock, Is.EqualTo(default(int)));
            Assert.That(moveNumber, Is.EqualTo(default(int)));
        }

        [TestCase("10 w - - 0 1", Pieces.White, null, null, null, 0, 1)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", Pieces.White, "White:0;White:7;Black:0;Black:7", null, null, 0, 1)]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah - 0 1", Pieces.White, "White:0;White:7;Black:0;Black:7", null, null, 0, 1)]
        [TestCase("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", Pieces.Black, "White:0;White:7;Black:0;Black:7", 4, 2, 0, 1)]
        [TestCase("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b AHah e3 0 1", Pieces.Black, "White:0;White:7;Black:0;Black:7", 4, 2, 0, 1)]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2", Pieces.White, "White:0;White:7;Black:0;Black:7", 2, 5, 0, 2)]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w AHah c6 0 2", Pieces.White, "White:0;White:7;Black:0;Black:7", 2, 5, 0, 2)]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2", Pieces.Black, "White:0;White:7;Black:0;Black:7", null, null, 1, 2)]
        [TestCase("rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b AHah - 1 2", Pieces.Black, "White:0;White:7;Black:0;Black:7", null, null, 1, 2)]
        [TestCase("8/8/8/8/8/8/8/8 w - - 0 1", Pieces.White, null, null, null, 0, 1)]
        [TestCase("rnbnkqrb/pppppppp/8/8/8/8/PPPPPPPP/RNBNKQRB w KQkq - 0 1", Pieces.White, "White:0;White:6;Black:0;Black:6", null, null, 0, 1, Description = "Wikipedia X-FEN")]
        [TestCase("rn2k1r1/ppp1pp1p/3p2p1/5bn1/P7/2N2B2/1PPPPP2/2BNK1RR w Gkq - 4 11", Pieces.White, "White:6;Black:0;Black:6", null, null, 4, 11, Description = "Wikipedia X-FEN")]
        public void TryParseXFen_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, Pieces expectedActivePlayer, string expectedCastlingStr, int? expectedEpX, int? expectedEpY, int expectedPlyCountClock, int expectedMoveNumber)
        {
            var expectedCastling = expectedCastlingStr == null
                ? null
                : expectedCastlingStr.Split(';').Select(s =>
                {
                    var parts = s.Split(':');
                    return Tuple.Create((Pieces)Enum.Parse(typeof(Pieces), parts[0]), int.Parse(parts[1]));
                }).ToList();
            var expectedEpCoordinate = expectedEpX == null || expectedEpY == null
                ? default(Point?)
                : new Point(expectedEpX.Value, expectedEpY.Value);

            var index = 0;
            var result = Parser.TryParseXFen(subject, ref index, out var board, out var activePlayer, out var castling, out var epCoordinate, out var plyCountClock, out var moveNumber);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(subject.Length));
            Assert.That(activePlayer, Is.EqualTo(expectedActivePlayer));

            if (expectedCastling == null)
            {
                Assert.That(castling, Is.Null);
            }
            else
            {
                Assert.That(castling, Is.EquivalentTo(expectedCastling));
            }

            Assert.That(epCoordinate, Is.EqualTo(expectedEpCoordinate));
            Assert.That(plyCountClock, Is.EqualTo(expectedPlyCountClock));
            Assert.That(moveNumber, Is.EqualTo(expectedMoveNumber));
        }

        [TestCase("", 0, 8)]
        [TestCase("//", 1, 8)]
        [TestCase("L", 0, 10)]
        [TestCase("Z", 0, 10)]
        [TestCase("l", 0, 10)]
        [TestCase("z", 0, 10)]
        [TestCase("B", 0, 1)]
        [TestCase("C", 0, 2)]
        [TestCase("D", 0, 3)]
        [TestCase("E", 0, 4)]
        [TestCase("F", 0, 5)]
        [TestCase("G", 0, 6)]
        [TestCase("H", 0, 7)]
        [TestCase("I", 0, 8)]
        [TestCase("J", 0, 9)]
        public void TryParseXFenCastling_WhenGivenInvalidInput_ReturnsFalse(string subject, int index, int boardWidth)
        {
            var startIndex = index;
            var result = Parser.TryParseXFenCastling(subject, ref index, new Pieces[boardWidth, 2], out var actual);
            Assert.That(result, Is.False);
            Assert.That(index, Is.EqualTo(startIndex));
            Assert.That(actual, Is.EqualTo(default(HashSet<Tuple<Pieces, int>>)));
        }

        [TestCase("A", 8, 0, 7, "White:0")]
        [TestCase("B", 8, 0, 7, "White:1")]
        [TestCase("C", 8, 0, 7, "White:2")]
        [TestCase("D", 8, 0, 7, "White:3")]
        [TestCase("E", 8, 0, 7, "White:4")]
        [TestCase("F", 8, 0, 7, "White:5")]
        [TestCase("G", 8, 0, 7, "White:6")]
        [TestCase("H", 8, 0, 7, "White:7")]
        [TestCase("J", 10, 0, 7, "White:9")]
        [TestCase("K", 8, 0, 7, "White:7")]
        [TestCase("Q", 8, 0, 7, "White:0")]
        [TestCase("K", 8, 1, 6, "White:6")]
        [TestCase("Q", 8, 1, 6, "White:1")]
        [TestCase("a", 8, 0, 7, "Black:0")]
        [TestCase("b", 8, 0, 7, "Black:1")]
        [TestCase("c", 8, 0, 7, "Black:2")]
        [TestCase("d", 8, 0, 7, "Black:3")]
        [TestCase("e", 8, 0, 7, "Black:4")]
        [TestCase("f", 8, 0, 7, "Black:5")]
        [TestCase("g", 8, 0, 7, "Black:6")]
        [TestCase("h", 8, 0, 7, "Black:7")]
        [TestCase("j", 10, 0, 7, "Black:9")]
        [TestCase("k", 8, 0, 7, "Black:7")]
        [TestCase("q", 8, 0, 7, "Black:0")]
        [TestCase("k", 8, 1, 6, "Black:6")]
        [TestCase("q", 8, 1, 6, "Black:1")]
        [TestCase("AHah", 8, 0, 7, "White:0;White:7;Black:0;Black:7")]
        [TestCase("KQkq", 8, 0, 7, "White:0;White:7;Black:0;Black:7")]
        [TestCase("KQkq", 8, 1, 6, "White:1;White:6;Black:1;Black:6")]
        [TestCase("KQkq", 4, 0, 3, "White:0;White:3;Black:0;Black:3")]
        [TestCase("KQkq", 4, 1, 2, "White:1;White:2;Black:1;Black:2")]
        [TestCase("qkQK", 8, 0, 7, "White:0;White:7;Black:0;Black:7")]
        [TestCase("qkQK", 8, 1, 6, "White:1;White:6;Black:1;Black:6")]
        [TestCase("qkQK", 4, 0, 3, "White:0;White:3;Black:0;Black:3")]
        [TestCase("qkQK", 4, 1, 2, "White:1;White:2;Black:1;Black:2")]
        public void TryParseXFenCastling_WhenGivenValidInput_ReturnsTheExpectedValue(string subject, int boardWidth, int fileQ, int fileK, string expectedStr)
        {
            var expected = expectedStr.Split(';').Select(s =>
            {
                var parts = s.Split(':');
                return Tuple.Create((Pieces)Enum.Parse(typeof(Pieces), parts[0]), int.Parse(parts[1]));
            }).ToList();

            var board = new Pieces[boardWidth, 2];
            board[fileQ, 0] = Pieces.White | Pieces.Rook;
            board[fileK, 0] = Pieces.White | Pieces.Rook;
            board[fileQ, 1] = Pieces.Black | Pieces.Rook;
            board[fileK, 1] = Pieces.Black | Pieces.Rook;

            var index = 0;
            var result = Parser.TryParseXFenCastling(subject, ref index, board, out var actual);
            Assert.That(result, Is.True);
            Assert.That(index, Is.EqualTo(subject.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}
