// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Games.LoveLetter.Actions;

    /// <summary>
    /// Represents a move to discard a card from the player's hand.
    /// </summary>
    public sealed class DiscardCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscardCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="cardIndex">The index of the card to discard in the player's hand.</param>
        public DiscardCardMove(GameState state, int cardIndex)
            : base(state)
        {
            this.CardIndex = cardIndex;
        }

        /// <summary>
        /// Gets the index of the card to discard.
        /// </summary>
        public int CardIndex { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.DiscardCard, this.GameState.Inventory[this.PlayerToken].Hand[this.CardIndex]);

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is DiscardCardMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.CardIndex.CompareTo(move.CardIndex)) != 0 ||
                    (comp = this.GameState.Inventory[this.PlayerToken].Hand[this.CardIndex].CompareTo(move.GameState.Inventory[move.PlayerToken].Hand[move.CardIndex])) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Players, move.GameState.Players)) != 0)
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

        internal static IEnumerable<DiscardCardMove> GenerateMoves(GameState state)
        {
            var hand = state.Inventory[state.ActivePlayer].Hand;
            if (state.Phase == Phase.Discard)
            {
                var containsCountess = hand.Contains(Card.Countess);
                for (var i = 0; i < hand.Length; i++)
                {
                    if (!containsCountess || (hand[i] != Card.King && hand[i] != Card.Prince))
                    {
                        yield return new DiscardCardMove(state, i);
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory;

            var activePlayerInventory = inventory[state.ActivePlayer];
            var hand = activePlayerInventory.Hand;
            var discard = hand[this.CardIndex];
            var discards = activePlayerInventory.Discards.Push(discard);
            hand = hand.RemoveAt(this.CardIndex);

            InterstitialState interstitial = null;
            switch (discard)
            {
                case Card.Guard:
                    interstitial = new GuardAction();
                    break;

                case Card.Priest:
                    interstitial = new PriestAction();
                    break;

                case Card.Baron:
                    interstitial = new BaronAction();
                    break;

                case Card.Prince:
                    interstitial = new PrinceAction();
                    break;

                case Card.King:
                    interstitial = new KingAction();
                    break;

                case Card.Princess:
                    discards = hand.Aggregate(discards, (d, c) => d.Push(c));
                    hand = ImmutableArray<Card>.Empty;
                    break;

                case Card.Handmaid:
                case Card.Countess:
                    break;
            }

            activePlayerInventory = activePlayerInventory.With(
                hand: hand,
                discards: discards);

            inventory = inventory.SetItem(
                state.ActivePlayer,
                activePlayerInventory);

            state = state.With(
                inventory: inventory);

            if (interstitial != null)
            {
                return state.WithInterstitialState(interstitial);
            }
            else
            {
                return base.Apply(state);
            }
        }
    }
}
