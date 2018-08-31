// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to reserve a development card from a development deck.
    /// </summary>
    public sealed class ReserveFromDeckMove : Move
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

        /// <inheritdoc />
        public override IList<object> FormatTokens => this.GameState.Tokens[Token.GoldJoker] > 0
            ? FormatUtilities.ParseStringFormat(Resources.ReserveFromDeckAndTakeJoker, this.Track + 1, Token.GoldJoker)
            : FormatUtilities.ParseStringFormat(Resources.ReserveFromDeck, this.Track + 1);

        /// <inheritdoc />
        public override bool IsDeterministic => false;

        /// <summary>
        /// Gets the index of the development track that contains the card to reserve.
        /// </summary>
        public int Track { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is ReserveFromDeckMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.Track.CompareTo(move.Track)) != 0)
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

        internal static IEnumerable<ReserveFromDeckMove> GenerateMoves(GameState state)
        {
            var pInventory = state.Inventory[state.ActivePlayer];

            if (pInventory.Hand.Count < GameState.HandLimit)
            {
                for (var tr = 0; tr < state.DevelopmentTracks.Length; tr++)
                {
                    if (state.DevelopmentDecks[tr].Count > 0)
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

            if (tokens[Token.GoldJoker] > 0)
            {
                tokens = tokens.Remove(Token.GoldJoker);
                pTokens = pTokens.Add(Token.GoldJoker);
            }

            deck = deck.Deal(out DevelopmentCard drawn);
            pHand = pHand.Add(drawn);

            return base.Apply(state.With(
                tokens: tokens,
                developmentDecks: state.DevelopmentDecks.SetItem(this.Track, deck),
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    hand: pHand,
                    tokens: pTokens))));
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            var tokens = state.Tokens;
            var deck = state.DevelopmentDecks[this.Track];
            var pInventory = state.Inventory[state.ActivePlayer];
            var pHand = pInventory.Hand;
            var pTokens = pInventory.Tokens;

            if (tokens[Token.GoldJoker] > 0)
            {
                tokens = tokens.Remove(Token.GoldJoker);
                pTokens = pTokens.Add(Token.GoldJoker);
            }

            for (var i = 0; i < deck.Count; i++)
            {
                var drawn = deck[i];
                var newDeck = deck.RemoveAt(i);
                var newHand = pHand.Add(drawn);

                var outcome = base.Apply(state.With(
                    tokens: tokens,
                    developmentDecks: state.DevelopmentDecks.SetItem(this.Track, newDeck),
                    inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                        hand: newHand,
                        tokens: pTokens))));

                yield return Weighted.Create(outcome, 1);
            }
        }
    }
}
