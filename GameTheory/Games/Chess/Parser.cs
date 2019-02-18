// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Parser
    {
        private const int DimensionX = 1;
        private const int DimensionY = 0;
        private const char FenEmptyFieldValue = '-';
        private const char FenRankSeparator = '/';
        private const char FenRecordSeparator = ' ';
        private const int MaxParseSize = 100;
        private const int MaxShredderFenBoardWidth = 'z' + 1 - 'a';
        private const int MaxXFenBoardWidth = 'k' - 'a';

        private static readonly Dictionary<char, Pieces> FenCastling = new Dictionary<char, Pieces>
        {
            ['q'] = Pieces.Black | Pieces.Queen,
            ['k'] = Pieces.Black | Pieces.King,
            ['Q'] = Pieces.White | Pieces.Queen,
            ['K'] = Pieces.White | Pieces.King,
        };

        private static readonly Dictionary<char, Pieces> FenColors = new Dictionary<char, Pieces>
        {
            ['b'] = Pieces.Black,
            ['w'] = Pieces.White,
        };

        private static readonly Dictionary<char, Pieces> FenPieces = new Dictionary<char, Pieces>
        {
            ['p'] = Pieces.Black | Pieces.Pawn,
            ['n'] = Pieces.Black | Pieces.Knight,
            ['b'] = Pieces.Black | Pieces.Bishop,
            ['r'] = Pieces.Black | Pieces.Rook,
            ['q'] = Pieces.Black | Pieces.Queen,
            ['k'] = Pieces.Black | Pieces.King,
            ['P'] = Pieces.White | Pieces.Pawn,
            ['N'] = Pieces.White | Pieces.Knight,
            ['B'] = Pieces.White | Pieces.Bishop,
            ['R'] = Pieces.White | Pieces.Rook,
            ['Q'] = Pieces.White | Pieces.Queen,
            ['K'] = Pieces.White | Pieces.King,
        };

        public static bool TryParseCoordinate(string subject, ref int index, out Point coordinate)
        {
            char c;
            if (index >= subject.Length || (c = subject[index]) < 'a' || c > 'z')
            {
                coordinate = default(Point);
                return false;
            }

            var startIndex = index;

            var file = subject[index++] - 'a';
            if (!Parser.TryParseInt32(subject, ref index, out var rank))
            {
                index = startIndex;
                coordinate = default(Point);
                return false;
            }
            else
            {
                coordinate = new Point(file, rank - 1);
                return true;
            }
        }

        public static bool TryParseFen(string subject, ref int index, out Pieces[,] board, out Pieces activePlayer, out HashSet<Pieces> castling, out Point? epCoordinate, out int plyCountClock, out int turnNumber)
        {
            var startIndex = index;
            if (!Parser.TryParseFenBoard(subject, ref index, out board) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseFenColor(subject, ref index, out activePlayer) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseFenCastlingField(subject, ref index, out castling) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseCoordinateField(subject, ref index, out epCoordinate) ||
                epCoordinate?.X >= board.GetLength(DimensionX) ||
                epCoordinate?.Y >= board.GetLength(DimensionY) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseInt32(subject, ref index, out plyCountClock) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseInt32(subject, ref index, out turnNumber))
            {
                index = startIndex;
                board = default(Pieces[,]);
                activePlayer = default(Pieces);
                castling = default(HashSet<Pieces>);
                epCoordinate = default(Point?);
                plyCountClock = default(int);
                turnNumber = default(int);
                return false;
            }

            return true;
        }

        public static bool TryParseFenBoard(string subject, ref int index, out Pieces[,] board)
        {
            var startIndex = index;
            var ranks = new List<List<Pieces>>();
            while (true)
            {
                if (!Parser.TryParseFenRank(subject, ref index, out var rank))
                {
                    if (ranks.Count == 0)
                    {
                        board = default(Pieces[,]);
                        return false;
                    }

                    index--;
                    break;
                }

                ranks.Add(rank);

                if (ranks.Count == MaxParseSize || !Parser.TryParseFenRankSeparator(subject, ref index))
                {
                    break;
                }
            }

            for (var r = 1; r < ranks.Count; r++)
            {
                if (ranks[r].Count != ranks[0].Count)
                {
                    index = startIndex;
                    board = default(Pieces[,]);
                    return false;
                }
            }

            var result = new Pieces[ranks.Count, ranks[0].Count];
            for (var r = 0; r < ranks.Count; r++)
            {
                var rank = ranks[ranks.Count - r - 1];
                for (var f = 0; f < rank.Count; f++)
                {
                    result[r, f] = rank[f];
                }
            }

            board = result;
            return true;
        }

        public static bool TryParseFenCastling(string subject, ref int index, out HashSet<Pieces> castling)
        {
            var startIndex = index;

            var result = new HashSet<Pieces>();
            while (index < subject.Length && Parser.FenCastling.TryGetValue(subject[index], out var value))
            {
                result.Add(value);
                index++;
            }

            if (result.Count == 0)
            {
                castling = default(HashSet<Pieces>);
                return false;
            }
            else
            {
                castling = result;
                return true;
            }
        }

        public static bool TryParseFenColor(string subject, ref int index, out Pieces color)
        {
            if (index >= subject.Length)
            {
                color = default(Pieces);
                return false;
            }

            if (Parser.FenColors.TryGetValue(subject[index], out color))
            {
                index++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryParseFenPiece(string subject, ref int index, out Pieces piece)
        {
            if (index >= subject.Length)
            {
                piece = default(Pieces);
                return false;
            }

            if (Parser.FenPieces.TryGetValue(subject[index], out piece))
            {
                index++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryParseFenRank(string subject, ref int index, out List<Pieces> pieces)
        {
            var startIndex = index;
            var result = new List<Pieces>();
            while (result.Count <= MaxParseSize)
            {
                if (Parser.TryParseFenPiece(subject, ref index, out var piece))
                {
                    result.Add(piece);
                }
                else if (Parser.TryParseInt32(subject, ref index, out var skip))
                {
                    if (result.Count + skip > MaxParseSize)
                    {
                        break;
                    }

                    result.AddRange(new Pieces[skip]);
                }
                else if (result.Count == 0)
                {
                    pieces = default(List<Pieces>);
                    return false;
                }
                else
                {
                    pieces = result;
                    return true;
                }
            }

            index = startIndex;
            pieces = default(List<Pieces>);
            return false;
        }

        public static bool TryParseFenRankSeparator(string subject, ref int index)
        {
            if (index < subject.Length && subject[index] == Parser.FenRankSeparator)
            {
                index++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryParseFenRecordSeparator(string subject, ref int index)
        {
            if (index < subject.Length && subject[index] == Parser.FenRecordSeparator)
            {
                index++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryParseInt32(string subject, ref int index, out int value)
        {
            var end = index;

            char c;
            while (end < subject.Length && (c = subject[end]) >= '0' && c <= '9')
            {
                end++;
            }

            if (end == index)
            {
                value = default(int);
                return false;
            }
            else if (int.TryParse(subject.Substring(index, end - index), out value))
            {
                index = end;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryParseShredderFen(string subject, ref int index, out Pieces[,] board, out Pieces activePlayer, out HashSet<Tuple<Pieces, int>> castling, out Point? epCoordinate, out int plyCountClock, out int turnNumber)
        {
            var startIndex = index;
            int boardWidth;
            if (!Parser.TryParseFenBoard(subject, ref index, out board) ||
                (boardWidth = board.GetLength(DimensionX)) > MaxShredderFenBoardWidth ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseFenColor(subject, ref index, out activePlayer) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseShredderFenCastlingField(subject, ref index, out castling) ||
                (castling != null && castling.Any(c => c.Item2 >= boardWidth)) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseCoordinateField(subject, ref index, out epCoordinate) ||
                epCoordinate?.X >= boardWidth ||
                epCoordinate?.Y >= board.GetLength(DimensionY) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseInt32(subject, ref index, out plyCountClock) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseInt32(subject, ref index, out turnNumber))
            {
                index = startIndex;
                board = default(Pieces[,]);
                activePlayer = default(Pieces);
                castling = default(HashSet<Tuple<Pieces, int>>);
                epCoordinate = default(Point?);
                plyCountClock = default(int);
                turnNumber = default(int);
                return false;
            }

            return true;
        }

        public static bool TryParseShredderFenCastling(string subject, ref int index, out HashSet<Tuple<Pieces, int>> castling)
        {
            var startIndex = index;
            var result = new HashSet<Tuple<Pieces, int>>();
            while (index < subject.Length)
            {
                char c;
                if ((c = subject[index]) >= 'a' && c <= 'z')
                {
                    index++;
                    result.Add(Tuple.Create(Pieces.Black, c - 'a'));
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    index++;
                    result.Add(Tuple.Create(Pieces.White, c - 'A'));
                }
                else
                {
                    break;
                }
            }

            if (result.Count == 0)
            {
                castling = default(HashSet<Tuple<Pieces, int>>);
                return false;
            }
            else
            {
                castling = result;
                return true;
            }
        }

        public static bool TryParseXFen(string subject, ref int index, out Pieces[,] board, out Pieces activePlayer, out HashSet<Tuple<Pieces, int>> castling, out Point? epCoordinate, out int plyCountClock, out int turnNumber)
        {
            var startIndex = index;
            int boardWidth;
            if (!Parser.TryParseFenBoard(subject, ref index, out board) ||
                (boardWidth = board.GetLength(DimensionX)) > MaxXFenBoardWidth ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseFenColor(subject, ref index, out activePlayer) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseXFenCastlingField(subject, ref index, board, out castling) ||
                (castling != null && castling.Any(c => c.Item2 >= boardWidth)) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseCoordinateField(subject, ref index, out epCoordinate) ||
                epCoordinate?.X >= boardWidth ||
                epCoordinate?.Y >= board.GetLength(DimensionY) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseInt32(subject, ref index, out plyCountClock) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseInt32(subject, ref index, out turnNumber))
            {
                index = startIndex;
                board = default(Pieces[,]);
                activePlayer = default(Pieces);
                castling = default(HashSet<Tuple<Pieces, int>>);
                epCoordinate = default(Point?);
                plyCountClock = default(int);
                turnNumber = default(int);
                return false;
            }

            return true;
        }

        public static bool TryParseXFenCastling(string subject, ref int index, Pieces[,] board, out HashSet<Tuple<Pieces, int>> castling)
        {
            int boardWidth;
            if (board == null)
            {
                throw new ArgumentOutOfRangeException(nameof(board));
            }
            else if ((boardWidth = board.GetLength(DimensionX)) == 0 || boardWidth > MaxXFenBoardWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(board));
            }

            var blackRank = board.GetLength(DimensionY) - 1;
            int? blackKingRook = null;
            int? blackQueenRook = null;
            int? whiteKingRook = null;
            int? whiteQueenRook = null;

            var startIndex = index;
            var result = new HashSet<Tuple<Pieces, int>>();
            while (index < subject.Length)
            {
                char c;
                if ((c = subject[index]) >= 'a' && c < 'a' + boardWidth)
                {
                    index++;
                    result.Add(Tuple.Create(Pieces.Black, c - 'a'));
                    continue;
                }
                else if (c >= 'A' && c < 'A' + boardWidth)
                {
                    index++;
                    result.Add(Tuple.Create(Pieces.White, c - 'A'));
                    continue;
                }
                else if (c == 'k')
                {
                    if (blackKingRook == null)
                    {
                        var stop = blackQueenRook ?? -1;
                        for (var i = boardWidth - 1; i > stop; i--)
                        {
                            if (board[blackRank, i] == (Pieces.Black | Pieces.Rook))
                            {
                                blackKingRook = i;
                                break;
                            }
                        }

                        if (blackKingRook == null)
                        {
                            index = startIndex;
                            result.Clear();
                            break;
                        }
                    }

                    index++;
                    result.Add(Tuple.Create(Pieces.Black, blackKingRook.Value));
                }
                else if (c == 'K')
                {
                    if (whiteKingRook == null)
                    {
                        var stop = whiteQueenRook ?? -1;
                        for (var i = boardWidth - 1; i > stop; i--)
                        {
                            if (board[0, i] == (Pieces.White | Pieces.Rook))
                            {
                                whiteKingRook = i;
                                break;
                            }
                        }

                        if (whiteKingRook == null)
                        {
                            index = startIndex;
                            result.Clear();
                            break;
                        }
                    }

                    index++;
                    result.Add(Tuple.Create(Pieces.White, whiteKingRook.Value));
                }
                else if (c == 'q')
                {
                    if (blackQueenRook == null)
                    {
                        var stop = blackKingRook ?? boardWidth;
                        for (var i = 0; i < stop; i++)
                        {
                            if (board[blackRank, i] == (Pieces.Black | Pieces.Rook))
                            {
                                blackQueenRook = i;
                                break;
                            }
                        }

                        if (blackQueenRook == null)
                        {
                            index = startIndex;
                            result.Clear();
                            break;
                        }
                    }

                    index++;
                    result.Add(Tuple.Create(Pieces.Black, blackQueenRook.Value));
                }
                else if (c == 'Q')
                {
                    if (whiteQueenRook == null)
                    {
                        var stop = whiteKingRook ?? boardWidth;
                        for (var i = 0; i < stop; i++)
                        {
                            if (board[0, i] == (Pieces.White | Pieces.Rook))
                            {
                                whiteQueenRook = i;
                                break;
                            }
                        }

                        if (whiteQueenRook == null)
                        {
                            index = startIndex;
                            result.Clear();
                            break;
                        }
                    }

                    index++;
                    result.Add(Tuple.Create(Pieces.White, whiteQueenRook.Value));
                }
                else
                {
                    break;
                }
            }

            if (result.Count == 0)
            {
                castling = default(HashSet<Tuple<Pieces, int>>);
                return false;
            }
            else
            {
                castling = result;
                return true;
            }
        }

        private static bool TryParseCoordinateField(string subject, ref int index, out Point? coordinate)
        {
            if (index >= subject.Length)
            {
                coordinate = default(Point?);
                return false;
            }
            else if (subject[index] == Parser.FenEmptyFieldValue)
            {
                coordinate = default(Point?);
                index++;
                return true;
            }

            var result = Parser.TryParseCoordinate(subject, ref index, out var coordinateValue);
            coordinate = coordinateValue;
            return result;
        }

        private static bool TryParseFenCastlingField(string subject, ref int index, out HashSet<Pieces> castling)
        {
            if (index >= subject.Length)
            {
                castling = default(HashSet<Pieces>);
                return false;
            }
            else if (subject[index] == Parser.FenEmptyFieldValue)
            {
                castling = default(HashSet<Pieces>);
                index++;
                return true;
            }

            return TryParseFenCastling(subject, ref index, out castling);
        }

        private static bool TryParseShredderFenCastlingField(string subject, ref int index, out HashSet<Tuple<Pieces, int>> castling)
        {
            if (index >= subject.Length)
            {
                castling = default(HashSet<Tuple<Pieces, int>>);
                return false;
            }
            else if (subject[index] == Parser.FenEmptyFieldValue)
            {
                castling = default(HashSet<Tuple<Pieces, int>>);
                index++;
                return true;
            }

            return TryParseShredderFenCastling(subject, ref index, out castling);
        }

        private static bool TryParseXFenCastlingField(string subject, ref int index, Pieces[,] board, out HashSet<Tuple<Pieces, int>> castling)
        {
            if (index >= subject.Length)
            {
                castling = default(HashSet<Tuple<Pieces, int>>);
                return false;
            }
            else if (subject[index] == Parser.FenEmptyFieldValue)
            {
                castling = default(HashSet<Tuple<Pieces, int>>);
                index++;
                return true;
            }

            return TryParseXFenCastling(subject, ref index, board, out castling);
        }
    }
}
