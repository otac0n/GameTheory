// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Moves
{
    using System.Collections.Generic;
    using GameTheory.Games.SevenDragons.Cards;

    /// <summary>
    /// Represents a move to play an action card.
    /// </summary>
    public sealed class PlayActionMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayActionMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="handIndex">The index of the card being played.</param>
        public PlayActionMove(GameState state, int handIndex)
            : base(state)
        {
            this.HandIndex = handIndex;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens =>
            FormatUtilities.ParseStringFormat(Resources.PlayActionCard, this.GameState.Inventories[this.GameState.InventoryMap[this.GameState.ActivePlayer]].Hand[this.HandIndex]);

        /// <summary>
        /// Gets the index of the card being played.
        /// </summary>
        public int HandIndex { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PlayActionMove playAction)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.HandIndex.CompareTo(playAction.HandIndex)) != 0)
                {
                    return comp;
                }

                var card = this.GameState.Inventories[this.GameState.InventoryMap[this.GameState.ActivePlayer]].Hand[this.HandIndex];
                var otherCard = playAction.GameState.Inventories[playAction.GameState.InventoryMap[playAction.GameState.ActivePlayer]].Hand[playAction.HandIndex];

                return card.CompareTo(otherCard);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerIndex = state.InventoryMap[this.PlayerToken];
            var playerInventory = state.Inventories[playerIndex];

            var actionCard = (ActionCard)playerInventory.Hand[this.HandIndex];
            playerInventory = playerInventory.With(
                hand: playerInventory.Hand.RemoveAt(this.HandIndex));

            state = state.With(
                discardPile: state.DiscardPile.Add(actionCard),
                inventories: state.Inventories.SetItem(
                    playerIndex,
                    playerInventory));

            return state.WithInterstitialState(new PlayingActionCard(actionCard));
        }

        private class PlayingActionCard : InterstitialState
        {
            public PlayingActionCard(ActionCard actionCard)
            {
                this.ActionCard = actionCard;
            }

            public ActionCard ActionCard { get; }

            public override int CompareTo(InterstitialState other)
            {
                if (other is PlayingActionCard playingActionCard)
                {
                    return this.ActionCard.CompareTo(playingActionCard.ActionCard);
                }
                else
                {
                    return base.CompareTo(other);
                }
            }

            public override IEnumerable<Move> GenerateMoves(GameState state) => this.ActionCard.GenerateActionMoves(state);
        }
    }
}
