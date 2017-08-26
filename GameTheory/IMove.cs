// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    /// <summary>
    /// Represents the contract of a move.
    /// </summary>
    public interface IMove
    {
        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        PlayerToken PlayerToken { get; }

        /// <summary>
        /// Gets a value indicating whether or not the immediate outcome of this move is certain.
        /// </summary>
        /// <remarks>
        /// A value of <c>true</c> means that the result of applying the move to a game state will always have the same outcome.
        /// A value of <c>false</c> means that the outcome may vary.
        /// </remarks>
        bool IsDeterministic { get; }
    }
}
