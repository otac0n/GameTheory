// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Games.LoveLetter.States;

    /// <summary>
    /// Represents a move to look at aonther player's hand.
    /// </summary>
    public sealed class LookAtHandMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LookAtHandMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="targetPlayer">The target player.</param>
        public LookAtHandMove(GameState state, PlayerToken targetPlayer)
            : base(state)
        {
            this.TargetPlayer = targetPlayer;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.LookAtHand, this.TargetPlayer);

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

            if (other is LookAtHandMove move)
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
            // TODO: Set flags for displaying hands.
            return state.WithInterstitialState(new PauseState());
        }
    }
}
