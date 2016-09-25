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
        /// <param name="track">The index of the development track that contains the card to reserve.</param>
        /// <param name="index">The index in the development track of the card to reserve.</param>
        public ReserveMove(GameState state, int track, int index)
            : base(state)
        {
            this.Track = track;
            this.Index = index;
        }

        /// <summary>
        /// Gets the development card to reserve.
        /// </summary>
        public DevelopmentCard Card => this.State.DevelopmentTracks[this.Track][this.Index];

        /// <summary>
        /// Gets the index in the development track of the card to reserve.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the index of the development track that contains the card to reserve.
        /// </summary>
        public int Track { get; }

        /// <inheritdoc />
        public override string ToString() => $"Reserve {this.Card}" + (this.State.Tokens[Token.GoldJoker] > 0 ? $" and take {Token.GoldJoker}" : string.Empty);

        internal override GameState Apply(GameState state)
        {
            var tokens = state.Tokens;
            var track = state.DevelopmentTracks[this.Track];
            var deck = state.DevelopmentDecks[this.Track];
            var pInventory = state.Inventory[state.ActivePlayer];
            var pHand = pInventory.Hand;
            var pTokens = pInventory.Tokens;

            pHand = pHand.Add(track[this.Index]);

            DevelopmentCard replacement;
            deck = deck.Deal(out replacement);
            track = track.SetItem(this.Index, replacement);

            if (tokens[Token.GoldJoker] > 0)
            {
                tokens = tokens.Remove(Token.GoldJoker);
                pTokens = pTokens.Add(Token.GoldJoker);
            }

            return base.Apply(state.With(
                tokens: tokens,
                developmentDecks: state.DevelopmentDecks.SetItem(this.Track, deck),
                developmentTracks: state.DevelopmentTracks.SetItem(this.Track, track),
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    hand: pHand,
                    tokens: pTokens))));
        }
    }
}
