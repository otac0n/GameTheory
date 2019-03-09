// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using GameTheory.Games.Mancala;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Mancala</see>.
    /// </summary>
    public class MancalaMaximizingPlayer : MaximizingPlayer<Move, ResultScore<double>>
    {
        private static readonly ResultScoringMetric<Move, double> Metric =
            new ResultScoringMetric<Move, double>(ScoringMetric.Create<Move>(Score));

        /// <summary>
        /// Initializes a new instance of the <see cref="MancalaMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public MancalaMaximizingPlayer(PlayerToken playerToken, int minPly = 9)
            : base(playerToken, Metric, minPly)
        {
        }

        /// <inheritdoc />
        protected override ICache MakeCache() => new Caches.NullCache();

        private static double Score(PlayerState<Move> playerState)
        {
            var state = (GameState)playerState.GameState;
            return state.Board[state.GetPlayerIndexOffset(state.Players.IndexOf(playerState.PlayerToken)) + state.BinsPerSide];
        }
    }
}
