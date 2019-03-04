// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a consistent interface for scoring functions.
    /// </summary>
    /// <typeparam name="TSubject">The type of subject to be scored.</typeparam>
    /// <typeparam name="TScore">The type used to represent the score.</typeparam>
    public interface IScoringMetric<TSubject, TScore> : IComparer<TScore>
    {
        /// <summary>
        /// Combines one or more scores using the specified weights.
        /// </summary>
        /// <param name="scores">The weighted scores to combine.</param>
        /// <returns>The combined score.</returns>
        /// <remarks>
        /// <para>If given a single score, that score should be returned without change.
        /// If given multiple scores, the function should return a expected score based on the specified weights.</para>
        /// </remarks>
        TScore Combine(params Weighted<TScore>[] scores);

        /// <summary>
        /// Gets the difference between two scores.
        /// </summary>
        /// <param name="minuend">The score to subtract from.</param>
        /// <param name="subtrahend">The score to subtract.</param>
        /// <returns>A score representing the difference between the specified scores.</returns>
        TScore Difference(TScore minuend, TScore subtrahend);

        /// <summary>
        /// Computes the score for the specified subject.
        /// </summary>
        /// <param name="subject">The subject to be scored.</param>
        /// <returns>The subject's score.</returns>
        TScore Score(TSubject subject);
    }
}
