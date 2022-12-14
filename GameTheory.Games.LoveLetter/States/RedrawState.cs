// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.States
{
    using System.Collections.Generic;
    using GameTheory.Games.LoveLetter.Moves;

    internal class RedrawState : InterstitialState
    {
        public RedrawState(PlayerToken playerToken)
        {
            this.PlayerToken = playerToken;
        }

        public PlayerToken PlayerToken { get; }

        public override int CompareTo(InterstitialState other)
        {
            if (other is RedrawState state)
            {
                return this.PlayerToken.CompareTo(state.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        public override IEnumerable<Move> GenerateMoves(GameState state)
        {
            yield return new DrawCardMove(state, this.PlayerToken);
        }
    }
}
