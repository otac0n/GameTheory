// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;

    /// <summary>
    /// A token allocated by a game representing a particular player in the game.
    /// </summary>
    public sealed class PlayerToken : IEquatable<PlayerToken>, IComparable<PlayerToken>
    {
        private readonly Guid id = Guid.NewGuid();

        /// <summary>
        /// Gets an identifier unique to this player.
        /// </summary>
        public Guid Id => this.id;

        /// <inheritdoc/>
        public int CompareTo(PlayerToken other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            return this.id.CompareTo(other.id);
        }

        /// <inheritdoc />
        public bool Equals(PlayerToken other)
        {
            return this == other;
        }

        /// <inheritdoc/>
        public override string ToString() => string.Format(SharedResources.PlayerTokenFormat, this.id.ToString().Split('-')[0]);
    }
}
