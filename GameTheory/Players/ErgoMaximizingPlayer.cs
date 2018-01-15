// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using Games.Ergo;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Tic tac toe</see>.
    /// </summary>
    public sealed class ErgoMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        private static readonly IScoringMetric<PlayerState, double> Metric = ScoringMetric.Create<PlayerState>(s => ((GameState)s.GameState).Scores[s.PlayerToken]);

        /// <summary>
        /// Initializes a new instance of the <see cref="ErgoMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public ErgoMaximizingPlayer(PlayerToken playerToken, int minPly = 6)
            : base(playerToken, Metric, minPly)
        {
        }
    }
}
