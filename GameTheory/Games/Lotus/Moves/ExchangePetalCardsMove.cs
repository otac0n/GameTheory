// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to exchange cards from the player deck.
    /// </summary>
    public class ExchangePetalCardsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangePetalCardsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="cardIndices">The indices of cards being exchanged.</param>
        public ExchangePetalCardsMove(GameState state, ImmutableList<int> cardIndices)
            : base(state)
        {
            this.CardIndices = cardIndices;
        }

        /// <summary>
        /// Gets the number of cards being exchanged.
        /// </summary>
        public ImmutableList<int> CardIndices { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens
        {
            get
            {
                var hand = this.State.Inventory[this.PlayerToken].Hand;
                return this.CardIndices.Count == 1
                    ? new object[] { "Exchange ", hand[this.CardIndices[0]] }
                    : new object[] { "Exchange ", hand[this.CardIndices[0]], " and ", hand[this.CardIndices[1]] };
            }
        }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is ExchangePetalCardsMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = CompareUtilities.CompareValueLists(this.CardIndices, move.CardIndices)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.State.Inventory[this.PlayerToken].Hand, move.State.Inventory[move.PlayerToken].Hand)) != 0)
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

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var inventory = state.Inventory[state.ActivePlayer];
            if (inventory.Deck.Count > 0)
            {
                var distinct = new HashSet<object>();

                for (var i1 = 0; i1 < inventory.Hand.Count; i1++)
                {
                    if (distinct.Add(inventory.Hand[i1]))
                    {
                        yield return new ExchangePetalCardsMove(state, ImmutableList.Create(i1));
                    }

                    if (inventory.Deck.Count > 1)
                    {
                        for (var i2 = 0; i2 < inventory.Hand.Count; i2++)
                        {
                            if (i1 == i2)
                            {
                                continue;
                            }

                            if (distinct.Add(Tuple.Create(inventory.Hand[i1], inventory.Hand[i2])))
                            {
                                yield return new ExchangePetalCardsMove(state, ImmutableList.Create(i1, i2));
                            }
                        }
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];
            var removed = this.CardIndices.Select(i => playerInventory.Hand[i]).ToList();

            foreach (var i in this.CardIndices.OrderByDescending(i => i))
            {
                var dealtIndex = playerInventory.Deck.Count - 1;
                var dealt = playerInventory.Deck[dealtIndex];

                playerInventory = playerInventory.With(
                    hand: playerInventory.Hand.SetItem(i, dealt),
                    deck: playerInventory.Deck.RemoveAt(dealtIndex));
            }

            playerInventory = playerInventory.With(
                deck: playerInventory.Deck.InsertRange(0, removed));

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory));

            return base.Apply(state);
        }
    }
}
