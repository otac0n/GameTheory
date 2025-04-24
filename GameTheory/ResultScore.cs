// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extends a score to prioritize winning over purely increasing the score.
    /// </summary>
    /// <typeparam name="TScore">The type that represents the rest of the score.</typeparam>
    public struct ResultScore<TScore> : ITokenFormattable, IEquatable<ResultScore<TScore>>
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

        /// <summary>
        /// Compares two <see cref="ResultScore{TScore}"/> objects. The result specifies whether they are unequal.
        /// </summary>
        /// <param name="left">The first <see cref="ResultScore{TScore}"/> to compare.</param>
        /// <param name="right">The second <see cref="ResultScore{TScore}"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(ResultScore<TScore> left, ResultScore<TScore> right) => !(left == right);

        /// <summary>
        /// Compares two <see cref="ResultScore{TScore}"/> objects. The result specifies whether they are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ResultScore{TScore}"/> to compare.</param>
        /// <param name="right">The second <see cref="ResultScore{TScore}"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(ResultScore<TScore> left, ResultScore<TScore> right) => left.Equals(right);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is ResultScore<TScore> other && this.Equals(other);

        /// <inheritdoc/>
        public bool Equals(ResultScore<TScore> other) =>
            this.Result == other.Result &&
            this.InPly == other.InPly &&
            this.Likelihood == other.Likelihood &&
            Equals(this.Rest, other.Rest);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, (short)this.Result);
            HashUtilities.Combine(ref hash, this.InPly.GetHashCode());
            HashUtilities.Combine(ref hash, this.Likelihood.GetHashCode());
            HashUtilities.Combine(ref hash, this.Rest.GetHashCode());
            return hash;
        }

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
