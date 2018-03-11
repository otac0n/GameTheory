// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to choose and remove a card from the challenging player's hand.
    /// </summary>
    public sealed class RemoveCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        /// <param name="removeIndex">The index of the presented card to remove.</param>
        public RemoveCardMove(GameState state, PlayerToken player, int removeIndex)
            : base(state, player)
        {
            this.RemoveIndex = removeIndex;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { $"Remove card #{this.RemoveIndex + 1}" };

        /// <summary>
        /// Gets the index of the presented card to remove.
        /// </summary>
        public int RemoveIndex { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is RemoveCardMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.RemoveIndex.CompareTo(move.RemoveIndex)) != 0 ||
                    (comp = this.GameState.ActivePlayer.CompareTo(move.GameState.ActivePlayer)) != 0 ||
                    (comp = this.GameState.Inventory[this.GameState.ActivePlayer].PresentedCards.Count.CompareTo(move.GameState.Inventory[move.GameState.ActivePlayer].PresentedCards.Count)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Players, move.GameState.Players)) != 0)
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

        internal static IEnumerable<RemoveCardMove> GenerateMoves(GameState state)
        {
            var presentedCards = state.Inventory[state.ActivePlayer].PresentedCards;
            if (presentedCards.Count > 0)
            {
                foreach (var player in state.Players)
                {
                    if (state.Inventory[player].Revealed[Card.Skull] > 0)
                    {
                        for (var i = 0; i < presentedCards.Count; i++)
                        {
                            yield return new RemoveCardMove(state, player, i);
                        }

                        break;
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory;

            var activePlayerInventory = inventory[state.ActivePlayer];
            activePlayerInventory = activePlayerInventory.With(
                bid: activePlayerInventory.PresentedCards.Count == 1 ? Inventory.PassingBid : 0,
                hand: new EnumCollection<Card>(activePlayerInventory.PresentedCards.RemoveAt(this.RemoveIndex)),
                discards: activePlayerInventory.Discards.Add(activePlayerInventory.PresentedCards[this.RemoveIndex]),
                presentedCards: ImmutableList<Card>.Empty);

            inventory = inventory.SetItem(
                state.ActivePlayer,
                activePlayerInventory);

            inventory = inventory.SetItems(
                from kvp in inventory
                let playerInventory = kvp.Value
                where playerInventory.Revealed.Count > 0 || playerInventory.Stack.Count > 0
                let newHand = playerInventory.Hand.AddRange(playerInventory.Revealed).AddRange(playerInventory.Stack)
                select new KeyValuePair<PlayerToken, Inventory>(kvp.Key, playerInventory.With(
                    bid: 0,
                    hand: newHand,
                    revealed: EnumCollection<Card>.Empty,
                    stack: ImmutableList<Card>.Empty)));

            state = state.With(
                phase: activePlayerInventory.Bid > Inventory.PassingBid || inventory.Count(i => i.Value.Hand.Count > 0) > 1
                    ? Phase.AddingCards
                    : Phase.End,
                activePlayer: activePlayerInventory.Bid > Inventory.PassingBid ? state.ActivePlayer : this.PlayerToken,
                inventory: inventory);

            return base.Apply(state);
        }
    }
}
