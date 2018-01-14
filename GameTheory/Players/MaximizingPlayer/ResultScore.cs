// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The possible outcomes for a game.
    /// </summary>
    public enum Result : short
    {
        /// <summary>
        /// The outcome has not yet been determined.
        /// </summary>
        None = 0,

        /// <summary>
        /// The game will result in a loss for the player.
        /// </summary>
        /// <remarks>
        /// A loss occurs when the game has no subsequent moves, the player is not a winner, and some other player is a winner.
        /// </remarks>
        Loss = -2,

        /// <summary>
        /// The game will result in an impasse.
        /// </summary>
        /// <remarks>
        /// An impasse (or stalemate) occurs when the game has no subsequent moves and there are no winners.
        /// </remarks>
        Impasse = -1,

        /// <summary>
        /// The game will result in a draw.
        /// </summary>
        /// <remarks>
        /// A draw occurs when the game has no subsequent moves, the player is a winner, and some other player is also a winner.
        /// </remarks>
        SharedWin = 1,

        /// <summary>
        /// The game will result in a win.
        /// </summary>
        /// <remarks>
        /// A win occurs when the game has no subsequen moves, the player is a winner, and no other player is a winner.
        /// </remarks>
        Win = 2,
    }

    /// <summary>
    /// Extends a score to prioritize winning over purely increasing the score.
    /// </summary>
    /// <typeparam name="TScore">The type that represents the rest of the score.</typeparam>
    public struct ResultScore<TScore> : ITokenFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultScore{TScore}"/> struct.
        /// </summary>
        /// <param name="result">The result of the game.</param>
        /// <param name="inPly">The number of moves until this result is reached.</param>
        /// <param name="likelihood">The liklihood of this result.</param>
        /// <param name="rest">The extended score.</param>
        public ResultScore(Result result, double inPly, double likelihood, TScore rest)
        {
            this.Result = result;
            this.InPly = inPly;
            this.Likelihood = likelihood;
            this.Rest = rest;
        }

        /// <inheritdoc/>
        public IList<object> FormatTokens => this.Result == Result.None
            ? new object[] { this.Rest }
            : this.Likelihood < 1
                ? new object[] { FormatLikelihood(this.Likelihood), " ", this.Result, " in ", this.InPly }
                : new object[] { this.Result, " in ", this.InPly };

        /// <summary>
        /// Gets the number of moves until this result is reached.
        /// </summary>
        public double InPly { get; }

        /// <summary>
        /// Gets the liklihood of this result.
        /// </summary>
        public double Likelihood { get; }

        /// <summary>
        /// Gets the extended score.
        /// </summary>
        public TScore Rest { get; }

        /// <summary>
        /// Gets the result of the game.
        /// </summary>
        public Result Result { get; }

        /// <inheritdoc/>
        public override string ToString() => string.Concat(this.FlattenFormatTokens());

        private static string FormatLikelihood(double value)
        {
            const int SignificantPlaces = 1;
            var d = (int)Math.Max(0, Math.Ceiling(-Math.Log10(value) - 3 + SignificantPlaces));
            var p = $"P{d}";
            var t = 1 - Math.Pow(10, -d - 2);
            return value < 1 && value > t
                ? ">" + t.ToString(p)
                : value.ToString(p);
        }
    }
}
