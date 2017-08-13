// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TwentyFortyEight.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a player move.
    /// </summary>
    public class ComputerMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputerMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="x">The x coordinate of the spot on which the move will me made.</param>
        /// <param name="y">The y coordinate of the spot on which the move will me made.</param>
        /// <param name="value">The value that will be added.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        private ComputerMove(GameState state, int x, int y, byte value)
            : base(state, state.Players[1])
        {
            this.X = x;
            this.Y = y;
            this.Value = value;
        }

        /// <summary>
        /// Gets the x coordinate of the spot on which the move will me made.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "X is meaningful in the context of coordinates.")]
        public int X { get; }

        /// <summary>
        /// Gets the y coordinate of the spot on which the move will me made.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y", Justification = "Y is meaningful in the context of coordinates.")]
        public int Y { get; }

        /// <summary>
        /// Gets the value that will be added.
        /// </summary>
        public byte Value { get; }

        /// <inheritdoc/>
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => $"Place {1 << this.Value} at ({this.X}, {this.Y})";

        internal static IEnumerable<Move> GetMoves(GameState state)
        {
            for (var y = 0; y < GameState.Size; y++)
            {
                for (var x = 0; x < GameState.Size; x++)
                {
                    if (state[x, y] == 0)
                    {
                        yield return new ComputerMove(state, x, y, GameState.SmallValue);
                        yield return new ComputerMove(state, x, y, GameState.LargeValue);
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var anyFound = false;
            var field = new byte[GameState.Size, GameState.Size];
            for (var y = 0; y < GameState.Size; y++)
            {
                for (var x = 0; x < GameState.Size; x++)
                {
                    var value = field[x, y] = state[x, y];
                    if (value != 0)
                    {
                        anyFound = true;
                    }
                }
            }

            field[this.X, this.Y] = this.Value;

            return state.With(turn: anyFound ? Turn.Player : Turn.Computer, field: field);
        }
    }
}
