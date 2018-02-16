// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Games.NormalFormGame;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState{T}">Mancala</see>.
    /// </summary>
    /// <typeparam name="T">The type representing the distint moves available.</typeparam>
    public class NormalFormGameMaximizingPlayer<T> : MaximizingPlayer<Move<T>, double>
        where T : class, IComparable<T>
    {
        private static readonly IScoringMetric<PlayerState, double> Metric = ScoringMetric.Create<PlayerState>(Score);

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalFormGameMaximizingPlayer{T}"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public NormalFormGameMaximizingPlayer(PlayerToken playerToken, int minPly = 4)
            : base(playerToken, Metric, minPly)
        {
        }

        /// <inheritdoc/>
        protected override ResultScore<double> GetLead(IDictionary<PlayerToken, ResultScore<double>> score, IGameState<Move<T>> state, PlayerToken player)
        {
            return score[player];
        }

        private static double Score(PlayerState playerState)
        {
            var state = (GameState<T>)playerState.GameState;
            return state.GetScore(playerState.PlayerToken);
        }
    }
}
