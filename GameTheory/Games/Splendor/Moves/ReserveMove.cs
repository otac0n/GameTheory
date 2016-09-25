// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    /// <summary>
    /// Represents a move to reserve a development card from the board.
    /// </summary>
    public class ReserveMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReserveMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="card">The development card to reserve.</param>
        public ReserveMove(GameState state, DevelopmentCard card)
            : base(state)
        {
            this.Card = card;
        }

        /// <summary>
        /// Gets the development card to reserve.
        /// </summary>
        public DevelopmentCard Card { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Reserve {this.Card}";
        }

        internal override GameState Apply(GameState state)
        {
            return base.Apply(state);
        }
    }
}
