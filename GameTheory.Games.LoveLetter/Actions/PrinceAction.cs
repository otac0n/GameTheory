// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Actions
{
    using System.Collections.Generic;
    using GameTheory.Games.LoveLetter.Moves;

    internal class PrinceAction : InterstitialState
    {
        public override IEnumerable<Move> GenerateMoves(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            foreach (var player in state.Players)
            {
                var otherInventory = state.Inventory[player];
                if (player == activePlayer || (otherInventory.Hand.Length > 0 && (otherInventory.Discards.IsEmpty || otherInventory.Discards.Peek() != Card.Handmaid)))
                {
                    yield return new DiscardHandMove(state, player);
                }
            }
        }
    }
}
