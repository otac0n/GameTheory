// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System.Linq;
    using Games.TicTacToe;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">Tic tac toe</see>.
    /// </summary>
    public sealed class TicTacToeMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        private static readonly IScoringMetric<PlayerState, double> scoringMetric = ScoringMetric.Create<PlayerState>(Score);

        /// <summary>
        /// Initializes a new instance of the <see cref="TicTacToeMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public TicTacToeMaximizingPlayer(PlayerToken playerToken, int minPly = 6)
            : base(playerToken, scoringMetric, minPly)
        {
        }

        private static double Score(PlayerState playerState)
        {
            var state = playerState.GameState;
            var playerToken = playerState.PlayerToken;
            return state.GetWinners().Any(w => w == playerToken) ? 1 : 0;
        }
    }
}
