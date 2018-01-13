// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using GameTheory.Games.Mancala;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">Mancala</see>.
    /// </summary>
    public class MancalaMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        private static readonly IScoringMetric<PlayerState, double> Metric = ScoringMetric.Create<PlayerState>(Score);

        /// <summary>
        /// Initializes a new instance of the <see cref="MancalaMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public MancalaMaximizingPlayer(PlayerToken playerToken, int minPly = 8)
            : base(playerToken, Metric, minPly)
        {
        }

        private static double Score(PlayerState playerState)
        {
            var state = (GameState)playerState.GameState;
            return state.Board[state.GetPlayerIndexOffset(playerState.PlayerToken) + state.BinsPerSide];
        }
    }
}
