// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to offer a card to another player.
    /// </summary>
    public sealed class OfferCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OfferCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        /// <param name="targetPlayer">The player being offered a card or cards.</param>
        /// <param name="actualCard">The card being offered.</param>
        /// <param name="claimedCard">The card being claimed.</param>
        public OfferCardMove(GameState state, PlayerToken player, PlayerToken targetPlayer, Card actualCard, Card claimedCard)
            : base(state, player)
        {
            this.TargetPlayer = targetPlayer;
            this.ActualCard = actualCard;
            this.ClaimedCard = claimedCard;
        }

        /// <summary>
        /// Get the player being offered a card or cards.
        /// </summary>
        public PlayerToken TargetPlayer { get; }

        /// <summary>
        /// Get the card being offered.
        /// </summary>
        public Card ActualCard { get; }

        /// <summary>
        /// Get the card being claimed.
        /// </summary>
        public Card ClaimedCard { get; }

        public override IList<object> FormatTokens => this.ClaimedCard == this.ActualCard
            ? FormatUtilities.ParseStringFormat(Resources.OfferCard, this.ActualCard, this.TargetPlayer)
            : FormatUtilities.ParseStringFormat(Resources.OfferCardAs, this.ActualCard, this.TargetPlayer, this.ClaimedCard);

        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is OfferCardMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.TargetPlayer.CompareTo(move.TargetPlayer)) != 0 ||
                    (comp = EnumComparer<Card>.Default.Compare(this.ActualCard, move.ActualCard)) != 0 ||
                    (comp = EnumComparer<Card>.Default.Compare(this.ClaimedCard, move.ClaimedCard)) != 0)
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

        internal static IEnumerable<OfferCardMove> GenerateMoves(GameState state)
        {
            if (state.OfferedCards.Count < GameState.MaxOfferedCards)
            {
                var activePlayer = state.TargetPlayer ?? state.FirstPlayer;
                var playerInventory = state.Inventory[activePlayer];
                var hand = playerInventory.Hand;
                foreach (var target in state.Players)
                {
                    if (target != activePlayer &&
                        state.Inventory[target].OwnedCards[Card.Charon] < GameState.PlayerCharonLimit &&
                        !state.OfferedCards.Any(o => target == o.SourcePlayer))
                    {
                        foreach (var card in hand.Keys)
                        {
                            if (card == Card.Charon)
                            {
                                foreach (var claimedCard in EnumUtilities<Card>.Values)
                                {
                                    yield return new OfferCardMove(state, activePlayer, target, card, claimedCard);
                                }
                            }
                            else
                            {
                                yield return new OfferCardMove(state, activePlayer, target, card, card);
                            }
                        }
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var activePlayer = this.PlayerToken;
            var playerInventory = state.Inventory[activePlayer];
            var actualCard = this.ActualCard;
            var claimedCard = this.ClaimedCard;
            var hand = playerInventory.Hand.Remove(actualCard);
            var offeredCards = state.OfferedCards.Add(new OfferedCard(actualCard, claimedCard, activePlayer));

            playerInventory = playerInventory.With(
                hand: hand);

            state = state.With(
                inventory: state.Inventory.SetItem(
                    activePlayer,
                    playerInventory),
                targetPlayer: this.TargetPlayer,
                offeredCards: offeredCards);

            return base.Apply(state);
        }
    }
}

