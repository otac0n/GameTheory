// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to reserve a development card from the board.
    /// </summary>
    public sealed class ReserveFromBoardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReserveFromBoardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="track">The index of the development track that contains the card to reserve.</param>
        /// <param name="index">The index in the development track of the card to reserve.</param>
        public ReserveFromBoardMove(GameState state, int track, int index)
            : base(state)
        {
            this.Track = track;
            this.Index = index;
        }

        /// <summary>
        /// Gets the development card to reserve.
        /// </summary>
        public DevelopmentCard Card => this.State.DevelopmentTracks[this.Track][this.Index];

        /// <inheritdoc />
        public override IList<object> FormatTokens => this.State.Tokens[Token.GoldJoker] > 0
            ? new object[] { "Reserve ", this.Card, " (cost: ", this.Card.Cost, ")", " and take ", Token.GoldJoker }
            : new object[] { "Reserve ", this.Card, " (cost: ", this.Card.Cost, ")" };

        /// <summary>
        /// Gets the index in the development track of the card to reserve.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => this.State.DevelopmentDecks[this.Track].Count <= 1;

        /// <summary>
        /// Gets the index of the development track that contains the card to reserve.
        /// </summary>
        public int Track { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is ReserveFromBoardMove move)
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

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var pInventory = state.Inventory[state.ActivePlayer];

            if (pInventory.Hand.Count < GameState.HandLimit)
            {
                for (var tr = 0; tr < state.DevelopmentTracks.Length; tr++)
                {
                    var track = state.DevelopmentTracks[tr];

                    for (var ix = 0; ix < track.Length; ix++)
                    {
                        if (track[ix] != null)
                        {
                            yield return new ReserveFromBoardMove(state, tr, ix);
                        }
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var tokens = state.Tokens;
            var track = state.DevelopmentTracks[this.Track];
            var deck = state.DevelopmentDecks[this.Track];
            var pInventory = state.Inventory[state.ActivePlayer];
            var pHand = pInventory.Hand;
            var pTokens = pInventory.Tokens;

            if (tokens[Token.GoldJoker] > 0)
            {
                tokens = tokens.Remove(Token.GoldJoker);
                pTokens = pTokens.Add(Token.GoldJoker);
            }

            pHand = pHand.Add(track[this.Index]);

            if (deck.Count == 0)
            {
                track = track.SetItem(this.Index, null);
            }
            else
            {
                deck = deck.Deal(out DevelopmentCard replacement);
                track = track.SetItem(this.Index, replacement);
            }

            return base.Apply(state.With(
                tokens: tokens,
                developmentDecks: state.DevelopmentDecks.SetItem(this.Track, deck),
                developmentTracks: state.DevelopmentTracks.SetItem(this.Track, track),
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    hand: pHand,
                    tokens: pTokens))));
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            var tokens = state.Tokens;
            var track = state.DevelopmentTracks[this.Track];
            var deck = state.DevelopmentDecks[this.Track];
            var pInventory = state.Inventory[state.ActivePlayer];
            var pHand = pInventory.Hand;
            var pTokens = pInventory.Tokens;

            if (tokens[Token.GoldJoker] > 0)
            {
                tokens = tokens.Remove(Token.GoldJoker);
                pTokens = pTokens.Add(Token.GoldJoker);
            }

            pHand = pHand.Add(track[this.Index]);

            if (deck.Count == 0)
            {
                track = track.SetItem(this.Index, null);

                var outcome = base.Apply(state.With(
                    tokens: tokens,
                    developmentDecks: state.DevelopmentDecks.SetItem(this.Track, deck),
                    developmentTracks: state.DevelopmentTracks.SetItem(this.Track, track),
                    inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                        hand: pHand,
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
                            hand: pHand,
                            tokens: pTokens))));
                    yield return Weighted.Create(outcome, 1);
                }
            }
        }
    }
}
