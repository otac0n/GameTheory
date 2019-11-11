// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    using System;
    using System.Collections.Generic;

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
                coordinate = default;
                return false;
            }

            var startIndex = index;

            var file = subject[index++] - 'a';
            if (!Parser.TryParseInt32(subject, ref index, out var rank))
            {
                index = startIndex;
                coordinate = default;
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
                board = default;
                activePlayer = default;
                castling = default;
                epCoordinate = default;
                plyCountClock = default;
                turnNumber = default;
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
                        board = default;
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
                    board = default;
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
                castling = default;
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
                color = default;
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
                piece = default;
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
                    pieces = default;
                    return false;
                }
                else
                {
                    pieces = result;
                    return true;
                }
            }

            index = startIndex;
            pieces = default;
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
                value = default;
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

        public static bool TryParseShredderFen(string subject, ref int index, out Pieces[,] board, out Pieces activePlayer, out Dictionary<Pieces, int> castling, out Point? epCoordinate, out int plyCountClock, out int turnNumber)
        {
            var startIndex = index;
            int boardWidth;
            if (!Parser.TryParseFenBoard(subject, ref index, out board) ||
                (boardWidth = board.GetLength(DimensionX)) > MaxShredderFenBoardWidth ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseFenColor(subject, ref index, out activePlayer) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseShredderFenCastlingField(subject, ref index, board, out castling) ||
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
                board = default;
                activePlayer = default;
                castling = default;
                epCoordinate = default;
                plyCountClock = default;
                turnNumber = default;
                return false;
            }

            return true;
        }

        public static bool TryParseShredderFenCastling(string subject, ref int index, Pieces[,] board, out Dictionary<Pieces, int> castling)
        {
            int boardWidth;
            if (board == null)
            {
                throw new ArgumentOutOfRangeException(nameof(board));
            }
            else if ((boardWidth = board.GetLength(DimensionX)) == 0 || boardWidth > MaxShredderFenBoardWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(board));
            }

            var blackRank = board.GetLength(DimensionY) - 1;
            int? blackKing = null;
            int? whiteKing = null;

            var startIndex = index;
            var result = new Dictionary<Pieces, int>();
            while (index < subject.Length)
            {
                char c;
                if ((c = subject[index]) >= 'a' && c <= 'z')
                {
                    var value = c - 'a';
                    if (value >= boardWidth || board[blackRank, value] != (Pieces.Black | Pieces.Rook))
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    if (blackKing == null)
                    {
                        for (var i = boardWidth - 1; i > -1; i--)
                        {
                            if (board[blackRank, i] == (Pieces.Black | Pieces.King))
                            {
                                blackKing = i;
                                break;
                            }
                        }

                        if (blackKing == null)
                        {
                            index = startIndex;
                            result.Clear();
                            break;
                        }
                    }

                    index++;
                    if (value > blackKing)
                    {
                        result.Add(Pieces.Black | Pieces.King, value);
                    }
                    else if (value < blackKing)
                    {
                        result.Add(Pieces.Black | Pieces.Queen, value);
                    }
                    else
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    continue;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    var value = c - 'A';
                    if (value >= boardWidth || board[blackRank, value] != (Pieces.Black | Pieces.Rook))
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    if (whiteKing == null)
                    {
                        for (var i = boardWidth - 1; i > -1; i--)
                        {
                            if (board[0, i] == (Pieces.White | Pieces.King))
                            {
                                whiteKing = i;
                                break;
                            }
                        }

                        if (whiteKing == null)
                        {
                            index = startIndex;
                            result.Clear();
                            break;
                        }
                    }

                    index++;
                    if (value > whiteKing)
                    {
                        result.Add(Pieces.White | Pieces.King, value);
                    }
                    else if (value < whiteKing)
                    {
                        result.Add(Pieces.White | Pieces.Queen, value);
                    }
                    else
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    continue;
                }
                else
                {
                    break;
                }
            }

            if (result.Count == 0)
            {
                castling = default;
                return false;
            }
            else
            {
                castling = result;
                return true;
            }
        }

        public static bool TryParseXFen(string subject, ref int index, out Pieces[,] board, out Pieces activePlayer, out Dictionary<Pieces, int> castling, out Point? epCoordinate, out int plyCountClock, out int turnNumber)
        {
            var startIndex = index;
            int boardWidth;
            if (!Parser.TryParseFenBoard(subject, ref index, out board) ||
                (boardWidth = board.GetLength(DimensionX)) > MaxXFenBoardWidth ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseFenColor(subject, ref index, out activePlayer) ||
                !Parser.TryParseFenRecordSeparator(subject, ref index) ||
                !Parser.TryParseXFenCastlingField(subject, ref index, board, out castling) ||
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
                board = default;
                activePlayer = default;
                castling = default;
                epCoordinate = default;
                plyCountClock = default;
                turnNumber = default;
                return false;
            }

            return true;
        }

        public static bool TryParseXFenCastling(string subject, ref int index, Pieces[,] board, out Dictionary<Pieces, int> castling)
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
            int? blackKing = null;
            int? blackKingRook = null;
            int? blackQueenRook = null;
            int? whiteKing = null;
            int? whiteKingRook = null;
            int? whiteQueenRook = null;

            var startIndex = index;
            var result = new Dictionary<Pieces, int>();
            while (index < subject.Length)
            {
                char c;
                if ((c = subject[index]) >= 'a' && c < 'a' + boardWidth)
                {
                    var value = c - 'a';
                    if (value >= boardWidth || board[blackRank, value] != (Pieces.Black | Pieces.Rook))
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    if (blackKing == null)
                    {
                        var stop = blackQueenRook ?? -1;
                        for (var i = boardWidth - 1; i > stop; i--)
                        {
                            if (board[blackRank, i] == (Pieces.Black | Pieces.King))
                            {
                                blackKing = i;
                                break;
                            }
                        }

                        if (blackKing == null)
                        {
                            index = startIndex;
                            result.Clear();
                            break;
                        }
                    }

                    index++;
                    if (value > blackKing && blackKingRook == null)
                    {
                        blackKingRook = value;
                        result.Add(Pieces.Black | Pieces.King, value);
                    }
                    else if (value < blackKing && blackQueenRook == null)
                    {
                        blackQueenRook = value;
                        result.Add(Pieces.Black | Pieces.Queen, value);
                    }
                    else
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    continue;
                }
                else if (c >= 'A' && c < 'A' + boardWidth)
                {
                    var value = c - 'A';
                    if (value >= boardWidth || board[blackRank, value] != (Pieces.Black | Pieces.Rook))
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    if (whiteKing == null)
                    {
                        var stop = whiteQueenRook ?? -1;
                        for (var i = boardWidth - 1; i > stop; i--)
                        {
                            if (board[0, i] == (Pieces.White | Pieces.King))
                            {
                                whiteKing = i;
                                break;
                            }
                        }

                        if (whiteKing == null)
                        {
                            index = startIndex;
                            result.Clear();
                            break;
                        }
                    }

                    index++;
                    if (value > whiteKing && whiteKingRook == null)
                    {
                        whiteKingRook = value;
                        result.Add(Pieces.White | Pieces.King, value);
                    }
                    else if (value < whiteKing && whiteQueenRook == null)
                    {
                        whiteQueenRook = value;
                        result.Add(Pieces.White | Pieces.Queen, value);
                    }
                    else
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    continue;
                }
                else if (c == 'k')
                {
                    if (blackKingRook == null)
                    {
                        var stop = Math.Max(blackQueenRook ?? -1, blackKing ?? -1);
                        for (var i = boardWidth - 1; i > stop; i--)
                        {
                            var piece = board[blackRank, i];
                            if (piece == (Pieces.Black | Pieces.King))
                            {
                                blackKing = i;
                                break;
                            }
                            else if (piece == (Pieces.Black | Pieces.Rook))
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
                    else
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    index++;
                    result.Add(Pieces.Black | Pieces.King, blackKingRook.Value);
                }
                else if (c == 'K')
                {
                    if (whiteKingRook == null)
                    {
                        var stop = Math.Max(whiteQueenRook ?? -1, whiteKing ?? -1);
                        for (var i = boardWidth - 1; i > stop; i--)
                        {
                            var piece = board[0, i];
                            if (piece == (Pieces.White | Pieces.King))
                            {
                                whiteKing = i;
                                break;
                            }
                            else if (piece == (Pieces.White | Pieces.Rook))
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
                    else
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    index++;
                    result.Add(Pieces.White | Pieces.King, whiteKingRook.Value);
                }
                else if (c == 'q')
                {
                    if (blackQueenRook == null)
                    {
                        var stop = Math.Min(blackKingRook ?? boardWidth, blackKing ?? boardWidth);
                        for (var i = 0; i < stop; i++)
                        {
                            var piece = board[blackRank, i];
                            if (piece == (Pieces.Black | Pieces.King))
                            {
                                blackKing = i;
                                break;
                            }
                            else if (piece == (Pieces.Black | Pieces.Rook))
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
                    else
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    index++;
                    result.Add(Pieces.Black | Pieces.Queen, blackQueenRook.Value);
                }
                else if (c == 'Q')
                {
                    if (whiteQueenRook == null)
                    {
                        var stop = Math.Min(whiteKingRook ?? boardWidth, whiteKing ?? boardWidth);
                        for (var i = 0; i < stop; i++)
                        {
                            var piece = board[0, i];
                            if (piece == (Pieces.White | Pieces.King))
                            {
                                whiteKing = i;
                                break;
                            }
                            else if (piece == (Pieces.White | Pieces.Rook))
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
                    else
                    {
                        index = startIndex;
                        result.Clear();
                        break;
                    }

                    index++;
                    result.Add(Pieces.White | Pieces.Queen, whiteQueenRook.Value);
                }
                else
                {
                    break;
                }
            }

            if (result.Count == 0)
            {
                castling = default;
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
                coordinate = default;
                return false;
            }
            else if (subject[index] == Parser.FenEmptyFieldValue)
            {
                coordinate = default;
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
                castling = default;
                return false;
            }
            else if (subject[index] == Parser.FenEmptyFieldValue)
            {
                castling = default;
                index++;
                return true;
            }

            return TryParseFenCastling(subject, ref index, out castling);
        }

        private static bool TryParseShredderFenCastlingField(string subject, ref int index, Pieces[,] board, out Dictionary<Pieces, int> castling)
        {
            if (index >= subject.Length)
            {
                castling = default;
                return false;
            }
            else if (subject[index] == Parser.FenEmptyFieldValue)
            {
                castling = default;
                index++;
                return true;
            }

            return TryParseShredderFenCastling(subject, ref index, board, out castling);
        }

        private static bool TryParseXFenCastlingField(string subject, ref int index, Pieces[,] board, out Dictionary<Pieces, int> castling)
        {
            if (index >= subject.Length)
            {
                castling = default;
                return false;
            }
            else if (subject[index] == Parser.FenEmptyFieldValue)
            {
                castling = default;
                index++;
                return true;
            }

            return TryParseXFenCastling(subject, ref index, board, out castling);
        }
    }
}
