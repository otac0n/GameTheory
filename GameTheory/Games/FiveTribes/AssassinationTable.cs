// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using System;

    /// <summary>
    /// Holds state regarding the assassination process.
    /// </summary>
    public class AssassinationTable : IComparable<AssassinationTable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinationTable"/> class.
        /// </summary>
        public AssassinationTable()
            : this(false, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinationTable"/> class.
        /// </summary>
        /// <param name="hasProtection">A value indicating whether or not the player's <see cref="Meeple">Meeples</see> have protection from assassination.</param>
        /// <param name="killCount">A number corresponding to the number of <see cref="Meeple">Meeples</see> that the player can kill during an assassination.</param>
        public AssassinationTable(bool hasProtection, int killCount)
        {
            this.HasProtection = hasProtection;
            this.KillCount = killCount;
        }

        /// <summary>
        /// Gets a value indicating whether or not the player's <see cref="Meeple">Meeples</see> have protection from assassination.
        /// </summary>
        public bool HasProtection { get; }

        /// <summary>
        /// Gets a number corresponding to the number of meeples that the player can kill during an assassination.
        /// </summary>
        public int KillCount { get; }

        /// <summary>
        /// Creates a new <see cref="AssassinationTable"/>, and updates the specified values.
        /// </summary>
        /// <param name="hasProtection"><c>null</c> to keep the existing value, or any other value to update <see cref="HasProtection"/>.</param>
        /// <param name="killCount"><c>null</c> to keep the existing value, or any other value to update <see cref="KillCount"/>.</param>
        /// <returns>The new <see cref="AssassinationTable"/>.</returns>
        public AssassinationTable With(bool? hasProtection = null, int? killCount = null)
        {
            return new AssassinationTable(
                hasProtection ?? this.HasProtection,
                killCount ?? this.KillCount);
        }

        /// <inheritdoc/>
        public int CompareTo(AssassinationTable other)
        {
            if (other == this)
            {
                return 0;
            }
            else if (other == null)
            {
                return 1;
            }

            int comp;

            if ((comp = this.KillCount.CompareTo(other.KillCount)) != 0 ||
                (comp = this.HasProtection.CompareTo(other.HasProtection)) != 0)
            {
                return comp;
            }

            return 0;
        }
    }
}
