// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

using System.Collections.Generic;

namespace GameTheory.Games.Splendor.Moves
{
    /// <summary>
    /// Represents a move to reserve a development card from the board or the player's hand.
    /// </summary>
    public class PurchaseMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="track">The index of the development track that contains the card to purchase.</param>
        /// <param name="index">The index in the development track of the card to purchase.</param>
        public PurchaseMove(GameState state, int track, int index)
            : base(state)
        {
            this.Track = track;
            this.Index = index;
        }

        /// <summary>
        /// Gets the development card to purchase.
        /// </summary>
        public DevelopmentCard Card => this.State.DevelopmentTracks[this.Track][this.Index];

        /// <summary>
        /// Gets the index in the development track of the card to purchase.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the index of the development track that contains the card to purchase.
        /// </summary>
        public int Track { get; }

        /// <inheritdoc />
        public override string ToString() => $"Purchase {this.Card} for {string.Join(",", this.Card.Cost)}";

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            // TODO: Yield moves that are possible given the user's bonuses.
            yield break;
        }

        internal override GameState Apply(GameState state)
        {
            var tokens = state.Tokens;
            var track = state.DevelopmentTracks[this.Track];
            var deck = state.DevelopmentDecks[this.Track];
            var card = track[this.Index];
            var pInventory = state.Inventory[state.ActivePlayer];
            var pDevelopmentCards = pInventory.DevelopmentCards;
            var pTokens = pInventory.Tokens;

            var cost = card.Cost;
            ////TODO: Provide a discount based on the user's bonuses.
            tokens = tokens.AddRange(cost);
            pTokens = pTokens.RemoveRange(cost);

            pDevelopmentCards.Add(card);

            DevelopmentCard replacement;
            deck = deck.Deal(out replacement);
            track = track.SetItem(this.Index, replacement);

            return base.Apply(state.With(
                tokens: tokens,
                developmentDecks: state.DevelopmentDecks.SetItem(this.Track, deck),
                developmentTracks: state.DevelopmentTracks.SetItem(this.Track, track),
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    developmentCards: pDevelopmentCards,
                    tokens: pTokens))));
        }
    }
}
