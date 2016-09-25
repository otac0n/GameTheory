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
        public override string ToString() => $"Reserve {this.Card}" + (this.State.Tokens[Token.GoldJoker] > 0 ? $" and take {Token.GoldJoker}" : string.Empty);

        internal override GameState Apply(GameState state)
        {
            // TODO: Replace card with one from deck or null.
            state = state.With(
                hand: state.Hand.SetItem(state.ActivePlayer, state.Hand[state.ActivePlayer].Add(this.Card)));

            if (state.Tokens[Token.GoldJoker] > 0)
            {
                state = state.With(
                    tokens: state.Tokens.Remove(Token.GoldJoker),
                    playerTokens: state.PlayerTokens.SetItem(state.ActivePlayer, state.PlayerTokens[state.ActivePlayer].Add(Token.GoldJoker)));
            }

            return base.Apply(state);
        }
    }
}
