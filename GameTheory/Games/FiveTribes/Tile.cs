// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Represents a tile in game of Five Tribes.
    /// </summary>
    public abstract class Tile : IComparable<Tile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        /// <param name="value">The value of the <see cref="Tile"/>, in Victory Points (VP).</param>
        /// <param name="color">The color of the <see cref="Tile"/>.</param>
        protected Tile(int value, TileColor color)
        {
            this.Value = value;
            this.Color = color;
        }

        /// <summary>
        /// Gets the color of the <see cref="Tile"/>.
        /// </summary>
        public TileColor Color { get; }

        /// <summary>
        /// Gets the value of the <see cref="Tile"/>, in Victory Points (VP).
        /// </summary>
        public int Value { get; }

        /// <inheritdoc/>
        public int CompareTo(Tile other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int comp;

            if ((comp = this.Value.CompareTo(other.Value)) != 0 ||
                (comp = this.Color.CompareTo(other.Color)) != 0 ||
                (comp = string.Compare(this.GetType().FullName, other.GetType().FullName, StringComparison.Ordinal)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <summary>
        /// Generates moves for the specified <see cref="GameState"/>.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> for which <see cref="Move">Moves</see> are being generated.</param>
        /// <returns>The <see cref="Move">Moves</see> provided by the <see cref="Tile"/>.</returns>
        public virtual IEnumerable<Move> GetTileActionMoves(GameState state)
        {
            yield return new ChangePhaseMove(state, "Skip Tile Action", Phase.MerchandiseSale);
        }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            return state.Sultanate[state.LastPoint].Tile.GetTileActionMoves(state);
        }
    }
}
