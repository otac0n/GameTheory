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
        /// <inheritdoc />
        public bool Equals(PlayerToken other)
        {
            return this == other;
        }
    }
}
