// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System;

    public struct WinCount
    {
        public WinCount(double wins, double simulations)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(simulations);

            this.Wins = wins;
            this.Simulations = simulations;
        }

        public double Ratio => this.Wins / this.Simulations;

        public double Simulations { get; }

        public double Wins { get; }

        /// <inheritdoc/>
        public override string ToString() => $"{this.Wins} / {this.Simulations}";
    }
}
