// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Serialization
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using GameTheory;
    using GameTheory.Games.Chess;
    using GameTheory.Games.Chess.NotationSystems;
    using GameTheory.GameTree;

    public static class Serializer
    {
        internal static readonly Dictionary<Pieces, char> FenColors = FenParser.FenColors.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        internal static readonly Dictionary<Pieces, char> FenPieces = FenParser.FenPieces.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static string SerializeFen(GameState state)
        {
            var sb = new StringBuilder();
            SerializeFen(sb, state);
            return sb.ToString();
        }

        public static void SerializeFen(StringBuilder sb, GameState state)
        {
            SerializeFenBoard(sb, state);
            sb.Append(FenParser.FenRecordSeparator);
            SerializeFenActivePlayer(sb, state);
            sb.Append(FenParser.FenRecordSeparator);
            SerializeFenCastling(sb, state);
            sb.Append(FenParser.FenRecordSeparator);
            SerializeFenEnPassantCoordinate(sb, state);
            sb.Append(FenParser.FenRecordSeparator);
            sb.Append(state.PlyCountClock);
            sb.Append(FenParser.FenRecordSeparator);
            sb.Append(state.MoveNumber);
        }

        public static void SerializeFenActivePlayer(StringBuilder sb, GameState state) =>
            SerializeFenColor(sb, state.ActiveColor);

        public static void SerializeFenBoard(StringBuilder sb, GameState state)
        {
            var first = true;
            for (var i = state.Variant.Height - 1; i >= 0; i--)
            {
                if (!first)
                {
                    sb.Append(FenParser.FenRankSeparator);
                }

                SerializeFenRank(sb, state, i);

                first = false;
            }
        }

        public static void SerializeFenCastling(StringBuilder sb, GameState state)
        {
            if (state.Castling.All(c => c < 0))
            {
                sb.Append(FenParser.FenEmptyFieldValue);
            }
            else
            {
                if (state.Castling[GameState.GetCastlingIndex(Pieces.White | Pieces.King)] >= 0)
                {
                    sb.Append('K');
                }

                if (state.Castling[GameState.GetCastlingIndex(Pieces.White | Pieces.Queen)] >= 0)
                {
                    sb.Append('Q');
                }

                if (state.Castling[GameState.GetCastlingIndex(Pieces.Black | Pieces.King)] >= 0)
                {
                    sb.Append('k');
                }

                if (state.Castling[GameState.GetCastlingIndex(Pieces.Black | Pieces.Queen)] >= 0)
                {
                    sb.Append('q');
                }
            }
        }

        public static void SerializeFenCastling(StringBuilder sb, ISet<Pieces> pieces)
        {
            if (pieces.Count == 0)
            {
                sb.Append(FenParser.FenEmptyFieldValue);
            }
            else
            {
                if (pieces.Contains(Pieces.White | Pieces.King))
                {
                    sb.Append('K');
                }

                if (pieces.Contains(Pieces.White | Pieces.Queen))
                {
                    sb.Append('Q');
                }

                if (pieces.Contains(Pieces.Black | Pieces.King))
                {
                    sb.Append('k');
                }

                if (pieces.Contains(Pieces.Black | Pieces.Queen))
                {
                    sb.Append('q');
                }
            }
        }

        public static void SerializeFenColor(StringBuilder sb, Pieces color)
        {
            sb.Append(FenColors[color]);
        }

        public static void SerializeFenCoordinate(StringBuilder sb, Point coordinate)
        {
            sb.Append((char)(coordinate.X + 'a'));
            sb.Append(coordinate.Y + 1);
        }

        public static void SerializeFenEnPassantCoordinate(StringBuilder sb, GameState state)
        {
            if (state.EnPassantIndex == null)
            {
                sb.Append(FenParser.FenEmptyFieldValue);
            }
            else
            {
                SerializeFenCoordinate(sb, state.Variant.GetCoordinates(state.EnPassantIndex.Value));
            }
        }

        public static void SerializePgn(TextWriter writer, IEnumerable<PgnGame> games, NotationSystem? notation = null)
        {
            notation ??= new AlgebraicNotation();

            foreach (var game in games)
            {
                SerializePgn(writer, game, notation);
            }
        }

        public static void SerializePgn(TextWriter writer, PgnGame game, NotationSystem? notation = null)
        {
            notation ??= new AlgebraicNotation();

            SerializePgnTags(writer, game.Tags);
            writer.WriteLine();
            SerializePgnElements(writer, game.Objects, notation);
            writer.WriteLine();
            writer.WriteLine();
        }

        private static void SerializeFenRank(StringBuilder sb, GameState state, int rank)
        {
            var skipped = 0;
            for (var i = 0; i < state.Variant.Width; i++)
            {
                var piece = state.GetPieceAt(i, rank);
                if (FenPieces.TryGetValue(piece, out var p))
                {
                    if (skipped > 0)
                    {
                        sb.Append(skipped);
                        skipped = 0;
                    }

                    sb.Append(p);
                }
                else
                {
                    skipped++;
                }
            }

            if (skipped > 0)
            {
                sb.Append(skipped);
            }
        }

        private static void SerializePgnTags(TextWriter writer, IEnumerable<KeyValuePair<string, string>> tags)
        {
            foreach (var tag in tags)
            {
                writer.Write('[');
                writer.Write(tag.Key);
                writer.Write(" \"");
                writer.Write(tag.Value.Replace("\\", "\\\\").Replace("\"", "\\\""));
                writer.WriteLine("\"]");
            }
        }

        private static void SerializePgnElements(TextWriter writer, IList<object> elements, NotationSystem notation)
        {
            var lastMove = 0;
            var needsDelimiter = false;

            foreach (var element in elements)
            {
                if (needsDelimiter)
                {
                    writer.Write(' ');
                    needsDelimiter = false;
                }

                switch (element)
                {
                    case IList<object> nested:
                        writer.Write('(');
                        SerializePgnElements(writer, nested, notation);
                        writer.Write(')');
                        lastMove = 0;
                        needsDelimiter = true;
                        break;

                    case Move move:
                        var state = move.GameState;
                        var moveNumber = state.MoveNumber;
                        if (moveNumber != lastMove)
                        {
                            writer.Write(moveNumber);
                            writer.Write(state.ActiveColor == Pieces.White ? "." : "...");
                            writer.Write(' ');
                            lastMove = moveNumber;
                        }

                        writer.Write(notation.FormatString(move));
                        if (move.IsCheckmate)
                        {
                            writer.Write('#');
                        }
                        else if (move.IsCheck)
                        {
                            writer.Write('+');
                        }

                        needsDelimiter = true;
                        break;

                    case NumericAnnotationGlyph glyph:
                        writer.Write('$');
                        writer.Write((int)glyph);
                        needsDelimiter = true;
                        break;

                    case string comment:
                        writer.Write('{');
                        writer.Write(comment);
                        writer.Write('}');
                        needsDelimiter = true;
                        break;

                    case Mainline<GameState, Move, Result> score:
                        var white = score.Scores[score.GameState.Players[0]];
                        var black = score.Scores[score.GameState.Players[1]];
                        writer.Write(
                            white is Result.Win && black is not Result.Win ? "1-0" :
                            black is Result.Win && white is not Result.Win ? "0-1" :
                            black is Result.Impasse && white is Result.Impasse ? "1/2-1/2" :
                            "*");
                        break;

                    default:
                        writer.WriteLine();
                        var text = element?.ToString() ?? "(null)";
                        foreach (var line in Regex.Split(text, "\r\n|\r|\n\r?"))
                        {
                            writer.Write('%');
                            writer.WriteLine(line);
                        }

                        break;
                }
            }
        }
    }
}
