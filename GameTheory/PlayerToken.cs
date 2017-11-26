// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;

    /// <summary>
    /// A token allocated by a game as a token representing a particular player in the game.
    /// </summary>
    public sealed class PlayerToken : IEquatable<PlayerToken>, IComparable<PlayerToken>
    {
        private readonly Guid id = Guid.NewGuid();

        /// <inheritdoc/>
        public int CompareTo(PlayerToken other)
        {
            return this == other ? 0 :
                other == null ? 1 :
                this.id.CompareTo(other.id);
        }

        /// <inheritdoc />
        public bool Equals(PlayerToken other)
        {
            return this == other;
        }

        /// <inheritdoc/>
        public override string ToString() => $"<Player #{this.id.ToString().Split('-')[0]}>";
    }
}
