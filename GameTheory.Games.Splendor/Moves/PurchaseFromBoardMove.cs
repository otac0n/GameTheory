﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to purchase a development card from the board.
    /// </summary>
    public sealed class PurchaseFromBoardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseFromBoardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="cost">The effective token cost of this card, including substituted jokers and bonuses.</param>
        /// <param name="track">The index of the development track that contains the card to purchase.</param>
        /// <param name="index">The index in the development track of the card to purchase.</param>
        public PurchaseFromBoardMove(GameState state, EnumCollection<Token> cost, int track, int index)
            : base(state)
        {
            this.Cost = cost;
            this.Track = track;
            this.Index = index;
        }

        /// <summary>
        /// Gets the development card to purchase.
        /// </summary>
        public DevelopmentCard Card => this.GameState.DevelopmentTracks[this.Track][this.Index];

        /// <summary>
        /// Gets the effective token cost of this card, including substituted jokers and bonuses.
        /// </summary>
        public EnumCollection<Token> Cost { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens => this.Cost.Count > 0
            ? FormatUtilities.ParseStringFormat(Resources.PurchaseCardForTokens, this.Card, this.Cost)
            : FormatUtilities.ParseStringFormat(Resources.PurchaseCardForFree, this.Card);

        /// <summary>
        /// Gets the index in the development track of the card to purchase.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => this.GameState.DevelopmentDecks[this.Track].Count <= 1;

        /// <summary>
        /// Gets the index of the development track that contains the card to purchase.
        /// </summary>
        public int Track { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PurchaseFromBoardMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.Track.CompareTo(move.Track)) != 0 ||
                    (comp = this.Index.CompareTo(move.Index)) != 0 ||
                    (comp = this.Card.CompareTo(move.Card)) != 0)
                {
                    return comp;
                }

                return 0;
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<PurchaseFromBoardMove> GenerateMoves(GameState state)
        {
            var tokens = state.Inventory[state.ActivePlayer].Tokens;
            var jokerCount = tokens[Token.GoldJoker];
            tokens = tokens.RemoveAll(Token.GoldJoker);

            var bonus = state.GetBonus(state.ActivePlayer);

            for (var t = 0; t < state.DevelopmentTracks.Length; t++)
            {
                for (var i = 0; i < state.DevelopmentTracks[t].Length; i++)
                {
                    var card = state.DevelopmentTracks[t][i];
                    if (card != null)
                    {
                        var cost = card.Cost.RemoveRange(bonus);
                        foreach (var tokenCost in GetAffordableTokenCosts(tokens, jokerCount, cost))
                        {
                            yield return new PurchaseFromBoardMove(state, tokenCost, t, i);
                        }
                    }
                }
            }
        }

        internal static IEnumerable<EnumCollection<Token>> GetAffordableTokenCosts(EnumCollection<Token> tokens, int jokerCount, EnumCollection<Token> cost)
        {
            var jokerCombinations = Enumerable.Concat(new[] { EnumCollection<Token>.Empty }, cost.Combinations(jokerCount, includeSmaller: true));
            foreach (var jokerCost in jokerCombinations)
            {
                var tokenCost = cost.RemoveRange(jokerCost);
                if (tokenCost.Keys.All(k => tokens[k] >= tokenCost[k]))
                {
                    yield return tokenCost.Add(Token.GoldJoker, jokerCost.Count);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var tokens = state.Tokens;
            var track = state.DevelopmentTracks[this.Track];
            var deck = state.DevelopmentDecks[this.Track];
            var pInventory = state.Inventory[state.ActivePlayer];
            var pDevelopmentCards = pInventory.DevelopmentCards;
            var pTokens = pInventory.Tokens;

            tokens = tokens.AddRange(this.Cost);
            pTokens = pTokens.RemoveRange(this.Cost);

            pDevelopmentCards = pDevelopmentCards.Add(track[this.Index]);

            if (deck.Count == 0)
            {
                track = track.SetItem(this.Index, null);
            }
            else
            {
                deck = deck.Deal(out var replacement);
                track = track.SetItem(this.Index, replacement);
            }

            return base.Apply(state.With(
                tokens: tokens,
                developmentDecks: state.DevelopmentDecks.SetItem(this.Track, deck),
                developmentTracks: state.DevelopmentTracks.SetItem(this.Track, track),
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    developmentCards: pDevelopmentCards,
                    tokens: pTokens))));
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            var tokens = state.Tokens;
            var track = state.DevelopmentTracks[this.Track];
            var deck = state.DevelopmentDecks[this.Track];
            var pInventory = state.Inventory[state.ActivePlayer];
            var pDevelopmentCards = pInventory.DevelopmentCards;
            var pTokens = pInventory.Tokens;

            tokens = tokens.AddRange(this.Cost);
            pTokens = pTokens.RemoveRange(this.Cost);

            pDevelopmentCards = pDevelopmentCards.Add(track[this.Index]);

            if (deck.Count == 0)
            {
                track = track.SetItem(this.Index, null);

                var outcome = base.Apply(state.With(
                    tokens: tokens,
                    developmentDecks: state.DevelopmentDecks.SetItem(this.Track, deck),
                    developmentTracks: state.DevelopmentTracks.SetItem(this.Track, track),
                    inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                        developmentCards: pDevelopmentCards,
                        tokens: pTokens))));
                yield return Weighted.Create(outcome, 1);
            }
            else
            {
                for (var i = 0; i < deck.Count; i++)
                {
                    var replacement = deck[i];
                    var newDeck = deck.RemoveAt(i);
                    var newTrack = track.SetItem(this.Index, replacement);

                    var outcome = base.Apply(state.With(
                        tokens: tokens,
                        developmentDecks: state.DevelopmentDecks.SetItem(this.Track, newDeck),
                        developmentTracks: state.DevelopmentTracks.SetItem(this.Track, newTrack),
                        inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                            developmentCards: pDevelopmentCards,
                            tokens: pTokens))));
                    yield return Weighted.Create(outcome, 1);
                }
            }
        }
    }
}
