// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree
{
    /// <summary>
    /// Provides the ability to extend a score by one ply.
    /// </summary>
    /// <typeparam name="TScore">The type used to keep track of the score.</typeparam>
    public interface IScorePlyExtender<TScore>
    {
        /// <summary>
        /// Extends the specified score by one ply.
        /// </summary>
        /// <param name="score">The score to extend.</param>
        /// <returns>The extended score.</returns>
        TScore Extend(TScore score);
    }
}
