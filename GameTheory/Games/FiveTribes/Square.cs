﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using GameTheory.Games.FiveTribes.Tiles;

    /// <summary>
    /// Represents a location in the <see cref="Sultanate"/>.
    /// </summary>
    public class Square
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Square"/> class.
        /// </summary>
        /// <param name="tile">The <see cref="Tile"/> at this <see cref="Square"/>.</param>
        /// <param name="meeples">The <see cref="Meeple">Meeples</see> at this <see cref="Square"/>.</param>
        public Square(Tile tile, EnumCollection<Meeple> meeples)
        {
            this.Tile = tile;
            this.Meeples = meeples;
        }

        private Square(EnumCollection<Meeple> meeples, PlayerToken owner, int palaces, int palmTrees, Tile tile)
        {
            this.Meeples = meeples;
            this.Owner = owner;
            this.Palaces = palaces;
            this.PalmTrees = palmTrees;
            this.Tile = tile;
        }

        /// <summary>
        /// Gets the <see cref="Meeple">Meeples</see> at this <see cref="Square"/>.
        /// </summary>
        public EnumCollection<Meeple> Meeples { get; }

        /// <summary>
        /// Gets the player who owns this <see cref="Square"/>.
        /// </summary>
        public PlayerToken Owner { get; }

        /// <summary>
        /// Gets the number of Palaces at this <see cref="Square"/>.
        /// </summary>
        public int Palaces { get; }

        /// <summary>
        /// Gets the number of Palm Trees at this <see cref="Square"/>.
        /// </summary>
        public int PalmTrees { get; }

        /// <summary>
        /// Gets the <see cref="Tile"/> at this <see cref="Square"/>.
        /// </summary>
        public Tile Tile { get; }

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
                meeples ?? this.Meeples,
                owner ?? this.Owner,
                palaces ?? this.Palaces,
                palmTrees ?? this.PalmTrees,
                tile ?? this.Tile);
        }
    }
}
