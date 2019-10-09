// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.States
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.LoveLetter.Moves;

    internal class ComparingHandsState : InterstitialState
    {
        public ComparingHandsState(PlayerToken targetPlayer)
        {
            this.TargetPlayer = targetPlayer;
        }

        public PlayerToken TargetPlayer { get; }

        public override int CompareTo(InterstitialState other)
        {
            if (other is ComparingHandsState state)
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
            var inventory = state.Inventory;
            var activePlayerCard = inventory[state.ActivePlayer].Hand.Single();
            var targetPlayerCard = inventory[this.TargetPlayer].Hand.Single();

            switch (EnumComparer<Card>.Default.Compare(activePlayerCard, targetPlayerCard))
            {
                case int i when i > 0:
                    yield return new DiscardHandMove(state, this.TargetPlayer, this.TargetPlayer, redraw: false);
                    break;

                case int i when i < 0:
                    yield return new DiscardHandMove(state, state.ActivePlayer, state.ActivePlayer, redraw: false);
                    break;

                default:
                    yield return new ContinueMove(state);
                    break;
            }
        }
    }
}
