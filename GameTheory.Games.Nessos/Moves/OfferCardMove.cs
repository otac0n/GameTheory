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
        /// <param name="targetPlayer">The player being offered a card or cards.</param>
        /// <param name="actualCard">The card being offered.</param>
        /// <param name="claimedCard">The card being claimed.</param>
        public OfferCardMove(GameState state, PlayerToken targetPlayer, Card actualCard, Card claimedCard)
            : base(state)
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

        public override IList<object> FormatTokens => throw new NotImplementedException();

        internal static IEnumerable<OfferCardMove> GenerateMoves(GameState state)
        {
            if (state.OfferedCards.Count < GameState.MaxOfferedCards)
            {
                var playerInventory = state.Inventory[state.ActivePlayer];
                var hand = playerInventory.Hand;
                foreach (var target in state.Players)
                {
                    if (target != state.ActivePlayer && !state.OfferedCards.Any(o => target == o.SourcePlayer))
                    {
                        foreach (var card in hand.Keys)
                        {
                            if (card == Card.Charon)
                            {
                                foreach (var claimedCard in EnumUtilities<Card>.Values)
                                {
                                    yield return new OfferCardMove(state, target, card, claimedCard);
                                }
                            }
                            else
                            {
                                yield return new OfferCardMove(state, target, card, card);
                            }
                        }
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];
            var actualCard = this.ActualCard;
            var claimedCard = this.ClaimedCard;
            var hand = playerInventory.Hand.Remove(actualCard);
            var offeredCards = state.OfferedCards.Add(new OfferedCard(actualCard, claimedCard, state.ActivePlayer));


            playerInventory = playerInventory.With(
                hand: hand);

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory),
                offeredCards: offeredCards);

            return base.Apply(state);
        }
    }
}

