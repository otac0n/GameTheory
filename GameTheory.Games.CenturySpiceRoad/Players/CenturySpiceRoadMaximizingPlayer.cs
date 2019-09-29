// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Players.MaximizingPlayers
{
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">CenturySpiceRoad</see>.
    /// </summary>
    public class CenturySpiceRoadMaximizingPlayer : MaximizingPlayer<Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<Move, double> Metric =
            ScoringMetric.Create((PlayerState<Move> s) => ((GameState)s.GameState).GetScore(s.PlayerToken));

        /// <summary>
        /// Initializes a new instance of the <see cref="CenturySpiceRoadMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public CenturySpiceRoadMaximizingPlayer(PlayerToken playerToken, int minPly, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
        }
    }
}