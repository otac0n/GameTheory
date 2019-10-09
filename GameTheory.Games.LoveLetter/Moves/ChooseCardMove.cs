// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System.Collections.Generic;
    using GameTheory.Games.LoveLetter.States;

    /// <summary>
    /// Represents a move to choose a target player.
    /// </summary>
    public sealed class ChooseCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChooseCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="targetPlayer">The target player.</param>
        /// <param name="targetCard">The target card.</param>
        public ChooseCardMove(GameState state, PlayerToken targetPlayer, Card targetCard)
            : base(state)
        {
            this.TargetPlayer = targetPlayer;
            this.TargetCard = targetCard;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.ChooseCard, this.TargetCard);

        /// <summary>
        /// Gets the target card.
        /// </summary>
        public Card TargetCard { get; }

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

            if (other is ChooseCardMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = EnumComparer<Card>.Default.Compare(this.TargetCard, move.TargetCard)) != 0 ||
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
            return state.WithInterstitialState(new GuardDiscardState(this.TargetPlayer, this.TargetCard));
        }
    }
}
