// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using GameTheory.Games.FiveTribes;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">Five Tribes</see>.
    /// </summary>
    public class FiveTribesMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        private static readonly IScoringMetric<PlayerState, double> Metric = ScoringMetric.Create<PlayerState>(s => ((GameState)s.GameState).GetScore(s.PlayerToken));

        /// <summary>
        /// Initializes a new instance of the <see cref="FiveTribesMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public FiveTribesMaximizingPlayer(PlayerToken playerToken, int minPly)
            : base(playerToken, Metric, minPly)
        {
        }
    }
}
