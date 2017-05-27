﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to reserve a development card from the board or the player's hand.
    /// </summary>
    public class PurchaseMove : Move
    {
        private static readonly ImmutableArray<Token> NonJokers = Enum.GetValues(typeof(Token)).Cast<Token>().Except(Token.GoldJoker).ToImmutableArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="cost">The effective token cost of this card, including substituted jokers and bonuses.</param>
        /// <param name="track">The index of the development track that contains the card to purchase.</param>
        /// <param name="index">The index in the development track of the card to purchase.</param>
        public PurchaseMove(GameState state, EnumCollection<Token> cost, int track, int index)
            : base(state)
        {
            this.Cost = cost;
            this.Track = track;
            this.Index = index;
        }

        /// <summary>
        /// Gets the development card to purchase.
        /// </summary>
        public DevelopmentCard Card => this.State.DevelopmentTracks[this.Track][this.Index];

        /// <summary>
        /// Gets the effective token cost of this card, including substituted jokers and bonuses.
        /// </summary>
        public EnumCollection<Token> Cost { get; }

        /// <summary>
        /// Gets the index in the development track of the card to purchase.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the index of the development track that contains the card to purchase.
        /// </summary>
        public int Track { get; }

        /// <inheritdoc />
        public override string ToString() => $"Purchase {this.Card} for {(this.Cost.Count > 0 ? this.Cost.ToString() : "free")}";

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var tokens = state.Inventory[state.ActivePlayer].Tokens;
            var jokerCount = tokens[Token.GoldJoker];
            tokens = tokens.RemoveAll(Token.GoldJoker);

            // TODO: May be more performant to move inside the inner loop to reduce total inner cycles.
            var jokers = new EnumCollection<Token>(NonJokers.SelectMany(n => Enumerable.Repeat(n, jokerCount)));
            var jokerCombinations = Enumerable.Concat(new[] { EnumCollection<Token>.Empty }, jokers.Combinations(jokerCount, includeSmaller: true));

            var bonus = state.GetBonus(state.ActivePlayer);

            for (int t = 0; t < state.DevelopmentTracks.Length; t++)
            {
                for (int i = 0; i < state.DevelopmentTracks[t].Length; i++)
                {
                    var card = state.DevelopmentTracks[t][i];
                    if (card != null)
                    {
                        var cost = card.Cost.RemoveRange(bonus);
                        foreach (var jokerCost in jokerCombinations.Where(jc => jc.Keys.All(k => cost[k] >= jc[k])))
                        {
                            var tokenCost = cost.RemoveRange(jokerCost);
                            if (tokenCost.Keys.All(k => tokens[k] >= tokenCost[k]))
                            {
                                yield return new PurchaseMove(state, tokenCost.Add(Token.GoldJoker, jokerCost.Count), t, i);
                            }
                        }
                    }
                }
            }

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

            tokens = tokens.AddRange(this.Cost);
            pTokens = pTokens.RemoveRange(this.Cost);

            pDevelopmentCards = pDevelopmentCards.Add(card);

            deck = deck.Deal(out DevelopmentCard replacement);
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
