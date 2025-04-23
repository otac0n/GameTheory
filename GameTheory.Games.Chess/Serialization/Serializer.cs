// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Serialization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using GameTheory;
    using GameTheory.Games.Chess;

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
    }
}
