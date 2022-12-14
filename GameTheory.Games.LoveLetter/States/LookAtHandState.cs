// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.States
{
    using System.Collections.Generic;
    using GameTheory.Games.LoveLetter.Moves;

    internal class LookAtHandState : InterstitialState
    {
        public LookAtHandState(PlayerToken targetPlayer)
        {
            this.TargetPlayer = targetPlayer;
        }

        public PlayerToken TargetPlayer { get; }

        public override IEnumerable<Move> GenerateMoves(GameState state)
        {
            yield return new ContinueMove(state);
        }
    }
}
