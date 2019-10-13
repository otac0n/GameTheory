// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Games.LoveLetter.States;

    /// <summary>
    /// Represents a move to discard a player's hand.
    /// </summary>
    public sealed class DiscardHandMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscardHandMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="targetPlayer">The target player whose hand will be discarded.</param>
        /// <param name="redraw">A value indicating whether or not the target player will redraw their hand.</param>
        public DiscardHandMove(GameState state, PlayerToken targetPlayer, bool redraw)
            : base(state)
        {
            this.TargetPlayer = targetPlayer;
            this.Redraw = redraw;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscardHandMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="player">The player doing the discarding.</param>
        /// <param name="targetPlayer">The target player whose hand will be discarded.</param>
        /// <param name="redraw">A value indicating whether or not the target player will redraw their hand.</param>
        public DiscardHandMove(GameState state, PlayerToken player, PlayerToken targetPlayer, bool redraw)
            : base(state, player)
        {
            this.TargetPlayer = targetPlayer;
            this.Redraw = redraw;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens =>
            this.PlayerToken == this.TargetPlayer
                ? FormatUtilities.ParseStringFormat(Resources.DiscardOwnHand)
                : FormatUtilities.ParseStringFormat(Resources.DiscardHand, this.TargetPlayer);

        /// <summary>
        /// Gets a value indicating whether or not the target player will redraw their hand.
        /// </summary>
        public bool Redraw { get; }

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
                    (comp = this.TargetPlayer.CompareTo(move.TargetPlayer)) != 0 ||
                    (comp = this.Redraw.CompareTo(move.Redraw)) != 0)
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
            if (this.Redraw && discard != Card.Princess)
            {
                interstitial = new RedrawState(this.TargetPlayer);
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
