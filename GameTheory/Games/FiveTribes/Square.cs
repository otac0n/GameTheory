// -----------------------------------------------------------------------
// <copyright file="Square.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    public class Square
    {
        private readonly EnumCollection<Meeple> meeples;
        private readonly PlayerToken owner;
        private readonly int palaces;
        private readonly int palmTrees;
        private readonly Tile tile;

        public Square(Tile tile, EnumCollection<Meeple> meeples)
        {
            this.tile = tile;
            this.meeples = meeples;
        }

        private Square(EnumCollection<Meeple> meeples, PlayerToken owner, int palaces, int palmTrees, Tile tile)
        {
            this.meeples = meeples;
            this.owner = owner;
            this.palaces = palaces;
            this.palmTrees = palmTrees;
            this.tile = tile;
        }

        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        public PlayerToken Owner
        {
            get { return this.owner; }
        }

        public int Palaces
        {
            get { return this.palaces; }
        }

        public int PalmTrees
        {
            get { return this.palmTrees; }
        }

        public Tile Tile
        {
            get { return this.tile; }
        }

        public Square With(EnumCollection<Meeple> meeples = null, PlayerToken owner = null, int? palaces = null, int? palmTrees = null, Tile tile = null)
        {
            return new Square(
                meeples ?? this.meeples,
                owner ?? this.owner,
                palaces ?? this.palaces,
                palmTrees ?? this.palmTrees,
                tile ?? this.tile);
        }
    }
}
