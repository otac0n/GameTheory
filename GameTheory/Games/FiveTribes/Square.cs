// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    /// <summary>
    /// Represents a location in the <see cref="Sultanate"/>.
    /// </summary>
    public class Square
    {
        private readonly EnumCollection<Meeple> meeples;
        private readonly PlayerToken owner;
        private readonly int palaces;
        private readonly int palmTrees;
        private readonly Tile tile;

        /// <summary>
        /// Initializes a new instance of the <see cref="Square"/> class.
        /// </summary>
        /// <param name="tile">The <see cref="Tile"/> at this <see cref="Square"/>.</param>
        /// <param name="meeples">The <see cref="Meeple">Meeples</see> at this <see cref="Square"/>.</param>
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

        /// <summary>
        /// Gets the <see cref="Meeple">Meeples</see> at this <see cref="Square"/>.
        /// </summary>
        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        /// <summary>
        /// Gets the player who owns this <see cref="Square"/>.
        /// </summary>
        public PlayerToken Owner
        {
            get { return this.owner; }
        }

        /// <summary>
        /// Gets the number of Palaces at this <see cref="Square"/>.
        /// </summary>
        public int Palaces
        {
            get { return this.palaces; }
        }

        /// <summary>
        /// Gets the number of Palm Trees at this <see cref="Square"/>.
        /// </summary>
        public int PalmTrees
        {
            get { return this.palmTrees; }
        }

        /// <summary>
        /// Gets the <see cref="Tile"/> at this <see cref="Square"/>.
        /// </summary>
        public Tile Tile
        {
            get { return this.tile; }
        }

        /// <summary>
        /// Creates a new <see cref="Square"/>, and updates the specified values.
        /// </summary>
        /// <param name="meeples"><c>null</c> to keep the existing value, or any other value to update <see cref="Meeples"/>.</param>
        /// <param name="owner"><c>null</c> to keep the existing value, or any other value to update <see cref="Owner"/>.</param>
        /// <param name="palaces"><c>null</c> to keep the existing value, or any other value to update <see cref="Palaces"/>.</param>
        /// <param name="palmTrees"><c>null</c> to keep the existing value, or any other value to update <see cref="PalmTrees"/>.</param>
        /// <param name="tile"><c>null</c> to keep the existing value, or any other value to update <see cref="Tile"/>.</param>
        /// <returns>The new <see cref="Square"/>.</returns>
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
