// -----------------------------------------------------------------------
// <copyright file="IMove.cs" company="(none)">
//   Copyright � 2014 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

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
        PlayerToken Player { get; }
    }
}
