// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.States
{
    using System.Collections.Generic;
    using GameTheory.Games.LoveLetter.Moves;

    internal class KingActionState : InterstitialState
    {
        public override IEnumerable<Move> GenerateMoves(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var anyTargets = false;
            foreach (var player in state.Players)
            {
                var otherInventory = state.Inventory[player];
                if (player != activePlayer && otherInventory.Hand.Length > 0 && (otherInventory.Discards.IsEmpty || otherInventory.Discards.Peek() != Card.Handmaid))
                {
                    anyTargets = true;
                    yield return new TradeHandsMove(state, player);
                }
            }

            if (!anyTargets)
            {
                yield return new ContinueMove(state);
            }
        }
    }
}
