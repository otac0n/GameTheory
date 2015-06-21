// -----------------------------------------------------------------------
// <copyright file="AssasinationTable.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    public class AssasinationTable
    {
        private readonly bool hasProtection;
        private readonly int killCount;

        public AssasinationTable()
            : this(false, 1)
        {
        }

        public AssasinationTable(bool hasProtection, int killCount)
        {
            this.hasProtection = hasProtection;
            this.killCount = killCount;
        }

        public bool HasProtection
        {
            get { return this.hasProtection; }
        }

        public int KillCount
        {
            get { return this.killCount; }
        }

        public AssasinationTable With(bool? hasProtection = null, int? killCount = null)
        {
            return new AssasinationTable(
                hasProtection ?? this.hasProtection,
                killCount ?? this.killCount);
        }
    }
}
