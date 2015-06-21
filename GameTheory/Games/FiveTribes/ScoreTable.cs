// -----------------------------------------------------------------------
// <copyright file="ScoreTable.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    public class ScoreTable
    {
        private readonly int builderMultiplier;
        private readonly int elderValue;
        private readonly int palaceValue;
        private readonly int palmTreeValue;
        private readonly int vizierValue;

        public ScoreTable()
        {
            this.builderMultiplier = 1;
            this.elderValue = 2;
            this.palaceValue = 5;
            this.palmTreeValue = 3;
            this.vizierValue = 1;
        }

        private ScoreTable(int builderMultiplier, int elderValue, int palaceValue, int palmTreeValue, int vizierValue)
        {
            this.builderMultiplier = builderMultiplier;
            this.elderValue = elderValue;
            this.palaceValue = palaceValue;
            this.palmTreeValue = palmTreeValue;
            this.vizierValue = vizierValue;
        }

        public int BuilderMultiplier
        {
            get { return this.builderMultiplier; }
        }

        public int ElderValue
        {
            get { return this.elderValue; }
        }

        public int PalaceValue
        {
            get { return this.palaceValue; }
        }

        public int PalmTreeValue
        {
            get { return this.palmTreeValue; }
        }

        public int VizierValue
        {
            get { return this.vizierValue; }
        }

        public ScoreTable With(int? builderMultiplier = null, int? elderValue = null, int? palaceValue = null, int? palmTreeValue = null, int? vizierValue = null)
        {
            return new ScoreTable(
                builderMultiplier ?? this.builderMultiplier,
                elderValue ?? this.elderValue,
                palaceValue ?? this.palaceValue,
                palmTreeValue ?? this.palmTreeValue,
                vizierValue ?? this.vizierValue);
        }
    }
}
