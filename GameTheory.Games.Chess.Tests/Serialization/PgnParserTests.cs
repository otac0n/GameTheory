// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Tests.Serialization
{
    using GameTheory.Games.Chess.NotationSystems;
    using GameTheory.Games.Chess.Serialization;
    using NUnit.Framework;

    internal class PgnParserTests
    {
        [Datapoints]
        public static readonly string[] ValidPgnFiles =
            [
                """
                *
                """,

                """
                1. 1/2-1/2
                """,

                """
                [Result "1-0"]
                d4 1-0
                """,

                """
                {a} 1. {b} e4 {c} e5 {d} 1/2-1/2
                """,

                """
                [Event "Casual bullet game"]
                [Site "https://lichess.org/vkyK3R2T"]
                [Date "2019.12.20"]
                [White "GoddessArtemis"]
                [Black "otac0n"]
                [Result "1-0"]
                [GameId "vkyK3R2T"]
                [UTCDate "2019.12.20"]
                [UTCTime "02:53:34"]
                [WhiteElo "635"]
                [BlackElo "1069"]
                [Variant "Standard"]
                [TimeControl "60+0"]
                [ECO "A06"]
                [Opening "Zukertort Opening"]
                [Termination "Time forfeit"]
                [Annotator "lichess.org"]

                1. Nf3 d5 { A06 Zukertort Opening } 2. g4 e5 3. Bh3 c5 4. Nxe5 Nf6 5. O-O Bd6
                6. Nd3 Qc7 7. f3 Bxh2+ 8. Kh1 c4 9. Nf4 Bxf4 10. e3 Bxe3 11. dxe3 d4 12. Qxd4 O-O
                13. b3 { White wins on time. } 1-0
                """,

                """
                [Event "Rated blitz game"]
                [Site "https://lichess.org/nr36ee5w"]
                [Date "2018.06.02"]
                [White "otac0n"]
                [Black "phoenigm"]
                [Result "0-1"]
                [GameId "nr36ee5w"]
                [UTCDate "2018.06.02"]
                [UTCTime "00:19:56"]
                [WhiteElo "1232"]
                [BlackElo "1398"]
                [WhiteRatingDiff "-6"]
                [BlackRatingDiff "+6"]
                [Variant "Standard"]
                [TimeControl "300+3"]
                [ECO "D06"]
                [Opening "Queen's Gambit Declined: Baltic Defense"]
                [Termination "Normal"]
                [Annotator "lichess.org"]

                1. d4 d5 2. c4 Bf5 { D06 Queen's Gambit Declined: Baltic Defense } 3. Nc3 e6
                4. e3 a6?! { (0.12 → 0.68) Inaccuracy. c6 was best. } (4... c6 5. Nf3 Nd7 6. Bd3 Bxd3 7. Qxd3 Bb4 8. e4 Ngf6 9. e5)
                5. Nge2 dxc4 6. Nf4 b5?! { (-0.04 → 0.99) Inaccuracy. Nf6 was best. } (6... Nf6 7. Bxc4 Bd6 8. O-O O-O 9. Re1 c5 10. dxc5 Be5 11. Bd3 Qc7 12. Bxf5 exf5 13. Qc2)
                7. e4?! { (0.99 → -0.06) Inaccuracy. g4 was best. } (7. g4 Bg6 8. h4 c5 9. Bg2 Ra7 10. h5 Bd3 11. Nxd3 cxd3 12. Qxd3 h6 13. a4 b4)
                7... Bg6 8. Nxg6 fxg6?? { (-0.71 → 1.00) Blunder. hxg6 was best. } (8... hxg6 9. Be2 Nf6 10. e5 Nd5 11. Nxd5 exd5 12. O-O c5 13. Bf3 Nc6 14. dxc5 Nb4 15. Be3)
                9. e5?? { (1.00 → -0.73) Blunder. a4 was best. } (9. a4 Nc6)
                9... Be7?? { (-0.73 → 1.16) Blunder. Nc6 was best. } (9... Nc6 10. Be3 Bb4 11. Qg4 Qd7 12. Be2 Nge7 13. O-O O-O 14. Rfd1 Nf5 15. a4 Nce7 16. Bg5)
                10. Be2? { (1.16 → -0.06) Mistake. Qf3 was best. } (10. Qf3 c6)
                10... c5? { (-0.06 → 1.38) Mistake. Nc6 was best. } (10... Nc6 11. Be3 Nh6 12. a4 O-O 13. O-O b4 14. Bxc4 bxc3 15. Bxe6+ Kh8 16. bxc3 Nf5 17. Bd2)
                11. d5 exd5 12. Nxd5 Nc6 13. f4?! { (0.72 → 0.01) Inaccuracy. a4 was best. } (13. a4 Nxe5 14. axb5 Nf6 15. Nxe7 Kxe7 16. bxa6 Qxd1+ 17. Kxd1 Nd5 18. Bg5+ Kd6 19. Bh4 c3)
                13... Qa5+? { (0.01 → 1.62) Mistake. Nh6 was best. } (13... Nh6 14. Bf3 Nf5 15. O-O O-O 16. a4 Rb8 17. Be4 Kh8 18. g3 Nfd4 19. axb5 axb5 20. Be3)
                14. Bd2 Qd8 15. Bc1? { (1.65 → 0.31) Mistake. Bf3 was best. } (15. Bf3 Rc8 16. b3 cxb3 17. axb3 Bh4+ 18. g3 Nge7 19. Nc3 Nf5 20. O-O Be7 21. Rxa6 Nb4)
                15... Nd4 16. Bf3 Nxf3+?? { (0.27 → 2.15) Blunder. Ra7 was best. } (16... Ra7 17. a4 b4 18. Ne3 Nxf3+ 19. Qxf3 Nh6 20. O-O O-O 21. Qc6 c3 22. bxc3 bxc3 23. Rd1)
                17. Qxf3 Bh4+?! { (2.07 → 3.22) Inaccuracy. Rb8 was best. } (17... Rb8 18. O-O)
                18. g3 Be7 19. Nxe7?? { (3.72 → 0.00) Blunder. Nc7+ was best. } (19. Nc7+ Qxc7)
                19... Nxe7 20. O-O?! { (0.00 → -0.95) Inaccuracy. Be3 was best. } (20. Be3 Rc8 21. Rd1 Qc7 22. O-O O-O 23. Rd6 Nf5 24. Rxa6 Rfd8 25. Bf2 Rd3 26. Qe4 Nd4)
                20... O-O?! { (-0.95 → -0.10) Inaccuracy. Qd5 was best. } (20... Qd5 21. Be3 Qxf3 22. Rxf3 Rc8 23. a4 O-O 24. Rd1 Nf5 25. axb5 axb5 26. Rd7 b4 27. Kg2)
                21. Be3 Rc8 22. Rfd1 Qa5?? { (-0.25 → 1.43) Blunder. Qc7 was best. } (22... Qc7)
                23. Rd7 Nf5?? { (0.99 → 4.25) Blunder. Rc7 was best. } (23... Rc7)
                24. Qd5+?? { (4.25 → 1.35) Blunder. g4 was best. } (24. g4 Kh8 25. gxf5 gxf5 26. Qg2 Rg8 27. Qc2 Rc6 28. h3 Qb4 29. Kh2 c3 30. Qxc3 Qe4)
                24... Kh8 25. Bxc5 Rcd8?? { (1.04 → 5.64) Blunder. Rg8 was best. } (25... Rg8 26. a3 Qa4 27. g4 Nh6 28. Bd4 Qc2 29. Qf3 Qd3 30. Kg2 Rcd8 31. Rxd8 Rxd8 32. Rd1)
                26. Bxf8 Qb6+ 27. Kh1 Rxf8 28. e6 Ne3?? { (5.36 → Mate in 6) Checkmate is now unavoidable. Re8 was best. } (28... Re8 29. Rd1)
                29. e7 Re8 30. Qf7?? { (Mate in 5 → -6.12) Lost forced checkmate sequence. Rd8 was best. } (30. Rd8 Qb8 31. Rxb8 h5 32. Rxe8+ Kh7 33. Qg8+ Kh6 34. Qh8#)
                30... Qc6+ 31. Kg1?? { (-6.11 → Mate in 1) Checkmate is now unavoidable. Rd5 was best. } (31. Rd5 Qxd5+ 32. Qxd5 Nxd5 33. Rd1 Nxe7 34. Kg1 b4 35. Rc1 Rc8 36. Kf2 Kg8 37. Ke3 Kf7)
                31... Qg2# { Black wins by checkmate. } 0-1
                """,

                // From https://www.saremba.de/chessgml/standards/pgn/pgn-complete.htm#c2.3
                // and https://github.com/fsmosca/PGN-Standard/blob/master/PGN-Standard.txt § 2.3
                """
                [Event "F/S Return Match"] 
                [Site "Belgrade, Serbia JUG"] 
                [Date "1992.11.04"] 
                [Round "29"] 
                [White "Fischer, Robert J."]
                [Black "Spassky, Boris V."] 
                [Result "1/2-1/2"] 

                1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 4. Ba4 Nf6 5. O-O Be7 6. Re1 b5 7. Bb3 d6 8. c3
                O-O 9. h3 Nb8 10. d4 Nbd7 11. c4 c6 12. cxb5 axb5 13. Nc3 Bb7 14. Bg5 b4 15.
                Nb1 h6 16. Bh4 c5 17. dxe5 Nxe4 18. Bxe7 Qxe7 19. exd6 Qf6 20. Nbd2 Nxd6 21.
                Nc4 Nxc4 22. Bxc4 Nb6 23. Ne5 Rae8 24. Bxf7+ Rxf7 25. Nxf7 Rxe1+ 26. Qxe1 Kxf7
                27. Qe3 Qg5 28. Qxg5 hxg5 29. b3 Ke6 30. a3 Kd6 31. axb4 cxb4 32. Ra5 Nd5 33.
                f3 Bc8 34. Kf2 Bf5 35. Ra7 g6 36. Ra6+ Kc5 37. Ke1 Nf4 38. g3 Nxh3 39. Kd2 Kb5
                40. Rd6 Kc5 41. Ra6 Nf2 42. g4 Bd3 43. Re6 1/2-1/2
                """,

                // From https://open-chess.org/viewtopic.php?t=1889
                """
                [Event "Wch1"]
                [Site "U.S.A."]
                [Date "1886.??.??"]
                [Round "9"]
                [White "Zukertort, Johannes"]
                [Black "Steinitz, Wilhelm"]
                [Result "0-1"]
                [ECO "D26h"]
                [Annotator "JvR"]

                1.d4 d5 2.c4 e6 3.Nc3 Nf6 4.Nf3 dxc4 5.e3 c5 6.Bxc4 cxd4 7.exd4 Be7 8.O-O
                O-O 9.Qe2 Nbd7 {This knight wants to blockades on d5.} 10.Bb3 Nb6 11.Bf4
                ( 11.Re1 {keeps the initiative.} )
                11...Nbd5 12.Bg3 Qa5 13.Rac1 Bd7 14.Ne5 Rfd8 15.Qf3 Be8 16.Rfe1 Rac8 17.
                Bh4 {Intends 18.Nxd5 exd5.} 17...Nxc3 18.bxc3 Qc7 {Black pressures on the
                hanging pawns.} 19.Qd3
                ( 19.Bg3 {!} 19...Bd6 20.c4 {(Lasker).} )
                19...Nd5 20.Bxe7 Qxe7 21.Bxd5 {?!}
                ( 21.c4 Qg5 22.Rcd1 Nf4 23.Qg3 {steers towards a slight advantage in
                the endgame.} )
                21...Rxd5 22.c4 Rdd8 23.Re3 {The attack will fail.}
                ( 23.Rcd1 {is solid.} )
                23...Qd6 24.Rd1 f6 25.Rh3 {!?} 25...h6 {!}
                ( 25...fxe5 26.Qxh7+ Kf8 27.Rg3 {!} 27...Rd7
                ( 27...Rc7 28.Qh8+ Ke7 29.Rxg7+ Bf7 30.Qh4+ {(Euwe)} )
                28.Qh8+ Ke7 29.Qh4+ Kf7 30.Qh7 {} )
                26.Ng4 Qf4 {!} 27.Ne3 Ba4 {!} 28.Rf3 Qd6 29.Rd2
                ( 29.Rxf6 {?} 29...Bxd1 {!} )
                29...Bc6 {?}
                ( 29...b5 {!} 30.Qg6 {!?}
                ( 30.cxb5 Rc1+ 31.Nd1 Qxd4 32.Qxd4 Rxd4 33.Rxd4 Bxd1 $19 {
                (Vukovic).} )
                30...Qf8 31.Ng4 Rxc4 {!} 32.Nxh6+ Kh8 33.h3 gxh6 34.Rxf6 Qg7 {is good
                for Black).} )
                30.Rg3 {?}
                ( 30.d5 {!} 30...Qe5 {!}
                ( 30...exd5 {(Steinitz)} 31.Nf5 {(Euwe)} )
                31.Qb1 {Forestalls ..b5 and protects the first rank.} 31...exd5 32.
                cxd5 {} 32...Bxd5 {??} 33.Rf5 )
                30...f5 {Threatens ..f4.} 31.Rg6 {!?}
                ( 31.Nd1 f4 32.Rh3 e5 {!} 33.d5 Bd7 $19 )
                31...Be4 32.Qb3 Kh7
                ( 32...Kf7 {(protects e6)} 33.c5 Qe7 {!} 34.Rg3 f4 )
                33.c5 Rxc5 34.Rxe6
                ( 34.Qxe6 Rc1+ $19 )
                34...Rc1+ 35.Nd1
                ( 35.Nf1 Qc7 $19 {!} )
                35...Qf4 36.Qb2 Rb1 37.Qc3 Rc8 {Utilises the unprotected first rank.} 38.
                Rxe4 Qxe4 {Many authors praise the high level of this positional game. The
                score had become 4-4. The match continued in New Orleans.} 0-1
                """
            ];

        [Theory]
        public void Parse_WithAllTestPGNFiles_SuccessfullyParsesTheFile(string pgnFile)
        {
            var parser = new PgnParser();
            var result = parser.Parse(pgnFile);
        }

        [Test]
        public void Parse_WithFigurineNotation_ParsesTheFile()
        {
            var game =
                """
                1. e4 e5 2. ♘f3 ♘c6 3. ♗c4 ♗c5 4. O-O ♘f6
                5. d4 ♗xd4 6. ♘xd4 ♘xd4 7. ♘c3 d6 8. ♗d5 ♘xd5
                9. exd5 e4 10. f4 exf3 11. ♖xf3 ♘xf3+ 12. ♕xf3 ♗f5
                13. ♗d2 ♕d7 14. ♗e3 O-O-O 15. a4 ♗xc2 16. a5 ♗d1
                17. ♗xa7 ♗xf3 18. ♗b6 ♗xg2 19. a6 ♗h1 20. a7 ♗xd5
                21. a8=♕# 1-0
                """;

            var parser = new PgnParser(new FigurineAlgebraicNotation());
            var result = parser.Parse(game);
        }
    }
}
