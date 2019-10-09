// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter
{
    using System.Collections.Generic;
    using GameTheory.Games.LoveLetter.Moves;

    internal class PauseState : InterstitialState
    {
        public override IEnumerable<Move> GenerateMoves(GameState state)
        {
            yield return new ContinueMove(state);
        }
    }
}
