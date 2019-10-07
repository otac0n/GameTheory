// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TwentyFortyEight.Moves
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// Represents a player move.
    /// </summary>
    public sealed class ComputerMove : Move
    {
        private GameState resultingState;

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

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.PlaceValue, 1 << this.Value, this.X, this.Y);

        /// <inheritdoc/>
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the value that will be added.
        /// </summary>
        public byte Value { get; }

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
            if (this.resultingState == null)
            {
                state = this.ApplyImpl(state);
                Interlocked.CompareExchange(ref this.resultingState, state, null);
            }

            return this.resultingState;
        }

        private GameState ApplyImpl(GameState state)
        {
            var field = state.Field;
            field[this.X, this.Y] = this.Value;
            return state.With(turn: Turn.Player, field: field);
        }
    }
}
