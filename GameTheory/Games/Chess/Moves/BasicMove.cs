// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move where a <see cref="Pieces">chessman</see> moves to an open square or a square occupied by an opposing pice.
    /// </summary>
    public class BasicMove : Move
    {
        private readonly GameState resultingState;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="fromIndex">The index of the square from which the <see cref="Pieces">chessman</see> is moving.</param>
        /// <param name="toIndex">The index of the square to which the <see cref="Pieces">chessman</see> is moving.</param>
        public BasicMove(GameState state, int fromIndex, int toIndex)
            : this(
                state,
                fromIndex,
                toIndex,
                Move.Advance(state.With(
                    board: state.Board
                        .SetItem(toIndex, state.Board[fromIndex])
                        .SetItem(fromIndex, Pieces.None))))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="fromIndex">The index of the square from which the <see cref="Pieces">chessman</see> is moving.</param>
        /// <param name="toIndex">The index of the square to which the <see cref="Pieces">chessman</see> is moving.</param>
        /// <param name="resultingState">The resulting board state.</param>
        protected BasicMove(GameState state, int fromIndex, int toIndex, GameState resultingState)
            : base(state)
        {
            this.FromIndex = fromIndex;
            this.ToIndex = toIndex;
            this.resultingState = resultingState;
        }

        /// <summary>
        /// Gets the index of the square from which the <see cref="Pieces">chessman</see> is moving.
        /// </summary>
        public int FromIndex { get; }

        /// <summary>
        /// Gets the index of the square to which the <see cref="Pieces">chessman</see> is moving.
        /// </summary>
        public int ToIndex { get; }

        internal override GameState Apply(GameState state) => this.resultingState;
    }
}
