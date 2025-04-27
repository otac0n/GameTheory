// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Players
{
    using System;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Suicide king player.
    /// </summary>
    /// <remarks>
    /// Inspired by Tom7 (suckerpinch)
    /// https://youtu.be/DpXy041BIlA?t=1050
    /// </remarks>
    public class SuicideKingPlayer : MaximizingPlayer<GameState, Move, ResultScore<double>>
    {
        private TimeSpan minThinkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="SuicideKingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="thinkSeconds">The minimum number of seconds to think.</param>
        public SuicideKingPlayer(PlayerToken playerToken, int minPly = 2, int thinkSeconds = 5)
            : base(playerToken, ResultScoringMetric.Create(ScoringMetric.Create<GameState, Move>(p => Score(playerToken, p)), misereMode: true), minPly)
        {
            this.minThinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds) - 0.1);
        }

        /// <inheritdoc/>
        protected override TimeSpan? MinThinkTime => this.minThinkTime;

        internal static double Score(PlayerToken chasingPlayer, PlayerState<GameState, Move> playerState)
        {
            var state = playerState.GameState;
            var board = state.Board;
            var colorDir = playerState.PlayerToken == chasingPlayer ? 1 : -1;

            var whiteKing = state.Variant.GetCoordinates(board.IndexOf(Pieces.White | Pieces.King));
            var blackKing = state.Variant.GetCoordinates(board.IndexOf(Pieces.Black | Pieces.King));

            return Math.Max(Math.Abs(whiteKing.X - blackKing.X), Math.Abs(whiteKing.Y - blackKing.Y)) * colorDir;
        }
    }
}
