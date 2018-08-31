// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TwentyFortyEight.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a player move.
    /// </summary>
    public sealed class PlayerMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="direction">The direction of the move.</param>
        private PlayerMove(GameState state, MoveDirection direction)
            : base(state, state.Players[0])
        {
            this.Direction = direction;
        }

        /// <summary>
        /// Gets the direction of the move.
        /// </summary>
        public MoveDirection Direction { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { this.Direction };

        /// <inheritdoc/>
        public override bool IsDeterministic => this.GameState.Players.Count != 1;

        internal static IEnumerable<Move> GetMoves(GameState state)
        {
            var left = false;
            var right = false;
            var up = false;
            var down = false;
            for (var i = 0; i < GameState.Size && !(left && right && up && down); i++)
            {
                for (var j = 0; j < GameState.Size - 1 && !(left && right && up && down); j++)
                {
                    var value = state[j, i];
                    var next = state[j + 1, i];
                    if (value != 0 && value == next)
                    {
                        left = true;
                        right = true;
                    }
                    else if (value != 0 && next == 0)
                    {
                        right = true;
                    }
                    else if (value == 0 && next != 0)
                    {
                        left = true;
                    }

                    value = state[i, j];
                    next = state[i, j + 1];
                    if (value != 0 && value == next)
                    {
                        up = true;
                        down = true;
                    }
                    else if (value != 0 && next == 0)
                    {
                        down = true;
                    }
                    else if (value == 0 && next != 0)
                    {
                        up = true;
                    }
                }
            }

            if (up)
            {
                yield return new PlayerMove(state, MoveDirection.Up);
            }

            if (right)
            {
                yield return new PlayerMove(state, MoveDirection.Right);
            }

            if (down)
            {
                yield return new PlayerMove(state, MoveDirection.Down);
            }

            if (left)
            {
                yield return new PlayerMove(state, MoveDirection.Left);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var field = new byte[GameState.Size, GameState.Size];

            int ix = 0, iy = 0, jx = 0, jy = 0, ox = 0, oy = 0;
            switch (this.Direction)
            {
                case MoveDirection.Up:
                    ix = 1;
                    jy = 1;
                    break;

                case MoveDirection.Right:
                    iy = 1;
                    jx = -1;
                    ox = GameState.Size - 1;
                    break;

                case MoveDirection.Down:
                    ix = 1;
                    jy = -1;
                    oy = GameState.Size - 1;
                    break;

                case MoveDirection.Left:
                    iy = 1;
                    jx = 1;
                    break;
            }

            for (var i = 0; i < GameState.Size; i++)
            {
                var j = 0;
                var jWrite = 0;

                byte peek() => state[i * ix + j * jx + ox, i * iy + j * jy + oy];

                void seek()
                {
                    while (j < GameState.Size && peek() == 0)
                    {
                        j++;
                    }
                }

                byte read()
                {
                    var value = peek();
                    j++;
                    seek();
                    return value;
                }

                seek();

                while (j < GameState.Size)
                {
                    var value = read();
                    if (j < GameState.Size && peek() == value)
                    {
                        read();
                        value++;
                    }

                    field[i * ix + jWrite * jx + ox, i * iy + jWrite * jy + oy] = value;
                    jWrite++;
                }
            }

            return state.With(turn: Turn.Computer, field: field);
        }
    }
}
