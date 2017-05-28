﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to reserve a development card from a development deck.
    /// </summary>
    public class ReserveFromDeckMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReserveFromDeckMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="track">The index of the development track that contains the card to reserve.</param>
        public ReserveFromDeckMove(GameState state, int track)
            : base(state)
        {
            this.Track = track;
        }

        /// <summary>
        /// Gets the index of the development track that contains the card to reserve.
        /// </summary>
        public int Track { get; }

        /// <inheritdoc />
        public override string ToString() => $"Reserve from deck {this.Track + 1}" + (this.State.Tokens[Token.GoldJoker] > 0 ? $" and take {Token.GoldJoker}" : string.Empty);

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var pInventory = state.Inventory[state.ActivePlayer];

            if (pInventory.Hand.Count < GameState.HandLimit)
            {
                for (var tr = 0; tr < state.DevelopmentTracks.Length; tr++)
                {
                    if (state.DevelopmentTracks[tr].Length > 0)
                    {
                        yield return new ReserveFromDeckMove(state, tr);
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var tokens = state.Tokens;
            var deck = state.DevelopmentDecks[this.Track];
            var pInventory = state.Inventory[state.ActivePlayer];
            var pHand = pInventory.Hand;
            var pTokens = pInventory.Tokens;

            deck = deck.Deal(out DevelopmentCard drawn);
            pHand = pHand.Add(drawn);

            if (tokens[Token.GoldJoker] > 0)
            {
                tokens = tokens.Remove(Token.GoldJoker);
                pTokens = pTokens.Add(Token.GoldJoker);
            }

            return base.Apply(state.With(
                tokens: tokens,
                developmentDecks: state.DevelopmentDecks.SetItem(this.Track, deck),
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    hand: pHand,
                    tokens: pTokens))));
        }
    }
}
