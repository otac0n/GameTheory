// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to choose another player to compare hands with.
    /// </summary>
    public sealed class DiscardHandMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscardHandMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="targetPlayer">The target player.</param>
        public DiscardHandMove(GameState state, PlayerToken targetPlayer)
            : base(state)
        {
            this.TargetPlayer = targetPlayer;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.DiscardHand, this.TargetPlayer);

        /// <summary>
        /// Gets the target player.
        /// </summary>
        public PlayerToken TargetPlayer { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is DiscardHandMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.TargetPlayer.CompareTo(move.TargetPlayer)) != 0)
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

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory;

            var targetInventory = inventory[this.TargetPlayer];
            var hand = targetInventory.Hand;
            var discard = hand.Single();
            var discards = targetInventory.Discards.Push(discard);
            hand = ImmutableArray<Card>.Empty;

            InterstitialState interstitial = null;
            if (discard != Card.Princess)
            {
                // TODO: Allow them to redraw.
            }

            targetInventory = targetInventory.With(
                hand: hand,
                discards: discards);

            inventory = inventory.SetItem(
                this.TargetPlayer,
                targetInventory);

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
