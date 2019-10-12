// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Games.Chess
{
    using System.Linq;
    using GameTheory.Games.Chess;
    using GameTheory.Games.Chess.Moves;
    using GameTheory.Testing;
    using NUnit.Framework;

    public class GameStateTests
    {
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", Description = "Starting Position in FEN and X-FEN")]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w HAha - 0 1", Description = "Starting Position in Shredder-FEN and X-FEN")]
        [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w AHah - 0 1", Description = "Starting Position in Shredder-FEN and X-FEN (castling reversed)")]
        [TestCase("rnbnkqrb/pppppppp/8/8/8/8/PPPPPPPP/RNBNKQRB w KQkq - 0 1", Description = "Wikipedia X-Fen Example 1")]
        [TestCase("rn2k1r1/ppp1pp1p/3p2p1/5bn1/P7/2N2B2/1PPPPP2/2BNK1RR w Gkq - 4 11", Description = "Wikipedia X-Fen Example 2")]
        [TestCase("rqkr/pppp/4/PPPP/RQKR w KQkq - 0 1", Description = "Silverman 4x5 X-FEN")]
        [TestCase("rqkr/pppp/4/PPPP/RQKR w ADad - 0 1", Description = "Silverman 4x5 Shredder-FEN and X-FEN")]
        [TestCase("rnbqk/ppppp/5/PPPPP/RNBQK w - - 0 1", Description = "Gardner 5x5")]
        [TestCase("r6nbqkbn6r/pppppppppppppppppppp/20/20/20/20/PPPPPPPPPPPPPPPPPPPP/R6NBQKBN6R w ATat - 0 1", Description = "Large Board Shredder-FEN")]
        public void ctor_WhenGivenAValidFENPosition_ResultsInAValidPosition(string position)
        {
            var state = new GameState(position);
        }

        [Test]
        public void GetAvailableMoves_After50MoveRule_YieldsNone()
        {
            var state = new GameState();

            for (var i = 0; i < 100; i++)
            {
                state = (GameState)state.PlayAnyMove(state.ActivePlayer, move =>
                {
                    if (move is BasicMove basicMove)
                    {
                        var piece = state[basicMove.FromIndex] & PieceMasks.Piece;
                        if (piece == Pieces.Knight || piece == Pieces.Rook)
                        {
                            if (state[basicMove.ToIndex] == Pieces.None)
                            {
                                return !move.IsCheck;
                            }
                        }
                    }

                    return false;
                });
            }

            Assert.That(state.GetWinners(), Is.Empty);
            Assert.That(state.GetAvailableMoves(), Is.Empty);
            Assert.That(state.PlyCountClock, Is.EqualTo(100));
            Assert.That(state.MoveNumber, Is.EqualTo(51));
        }

        [TestCase("r3k2r/8/8/8/8/8/4p3/R3K2R w KQkq - 0 1")]
        public void GetAvailableMoves_WhenCastlingWouldResultInCheck_DoesNotAllowCastling(string position)
        {
            var state = new GameState(position);
            Assert.That(state.GetAvailableMoves().OfType<CastleMove>(), Is.Empty);
        }

        [TestCase("1. e4; 1... d5; 2. exd5; 2... Qxd5; 3. Nf3; 3... Nc6; 4. Bd3; 4... Qe4+")]
        public void GetAvailableMoves_WhenInCheck_DoesNotAllowCastling(string moves)
        {
            var state = new GameState();
            foreach (var m in moves.Split(';').Select(m => m.Trim()))
            {
                state = (GameState)state.PlayMove(state.ActivePlayer, move => move.ToString() == m);
            }

            Assert.That(state.GetAvailableMoves().OfType<CastleMove>(), Is.Empty);
        }
    }
}
