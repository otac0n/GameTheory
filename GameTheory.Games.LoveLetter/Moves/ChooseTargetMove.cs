// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to choose a target player.
    /// </summary>
    public sealed class ChooseTargetMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChooseTargetMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="targetPlayer">The target player.</param>
        public ChooseTargetMove(GameState state, PlayerToken targetPlayer)
            : base(state)
        {
            this.TargetPlayer = targetPlayer;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.ChooseTarget, this.TargetPlayer);

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

            if (other is ChooseTargetMove move)
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
            // TODO: Choose a card.
            // TODO: Force the victim to discard their hand if the named card is correct.
            return base.Apply(state);
        }
    }
}
