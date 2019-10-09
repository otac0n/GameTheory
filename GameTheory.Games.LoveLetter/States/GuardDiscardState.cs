// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.States
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.LoveLetter.Moves;

    internal class GuardDiscardState : InterstitialState
    {
        public GuardDiscardState(PlayerToken targetPlayer, Card targetCard)
        {
            this.TargetPlayer = targetPlayer;
            this.TargetCard = targetCard;
        }

        public Card TargetCard { get; }

        public PlayerToken TargetPlayer { get; }

        public override int CompareTo(InterstitialState other)
        {
            if (other is GuardDiscardState state)
            {
                int comp;

                if ((comp = EnumComparer<Card>.Default.Compare(this.TargetCard, state.TargetCard)) != 0 ||
                    (comp = this.TargetPlayer.CompareTo(state.TargetPlayer)) != 0)
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

        public override IEnumerable<Move> GenerateMoves(GameState state)
        {
            var inventory = state.Inventory;
            var targetPlayerCard = inventory[this.TargetPlayer].Hand.Single();
            if (targetPlayerCard == this.TargetCard)
            {
                yield return new DiscardHandMove(state, this.TargetPlayer, this.TargetPlayer, redraw: false);
            }
            else
            {
                yield return new ContinueMove(state);
            }
        }
    }
}
