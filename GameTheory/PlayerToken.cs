// -----------------------------------------------------------------------
// <copyright file="PlayerToken.cs" company="(none)">
//   Copyright © 2014 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory
{
    using System;

    /// <summary>
    /// A token allocated by a game as a token representing a particular player in the game.
    /// </summary>
    public sealed class PlayerToken : IEquatable<PlayerToken>
    {
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(PlayerToken other)
        {
            return this == other;
        }
    }
}
