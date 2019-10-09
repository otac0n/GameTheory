// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.States
{
    using System.Collections.Generic;
    using GameTheory.Games.LoveLetter.Moves;

    internal class ChoosingCardState : InterstitialState
    {
        public ChoosingCardState(PlayerToken targetPlayer)
        {
            this.TargetPlayer = targetPlayer;
        }

        public PlayerToken TargetPlayer { get; }

        public override int CompareTo(InterstitialState other)
        {
            if (other is ChoosingCardState state)
            {
                return this.TargetPlayer.CompareTo(state.TargetPlayer);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        public override IEnumerable<Move> GenerateMoves(GameState state)
        {
            foreach (var card in EnumUtilities<Card>.GetValues())
            {
                if (card > Card.Guard)
                {
                    yield return new ChooseCardMove(state, this.TargetPlayer, card);
                }
            }
        }
    }
}
