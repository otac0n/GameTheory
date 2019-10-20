// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Comparers;

    /// <summary>
    /// Represents a move to exchange cards from the player deck.
    /// </summary>
    public sealed class ExchangePetalCardsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangePetalCardsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="cardIndexes">The indexes of cards being exchanged.</param>
        public ExchangePetalCardsMove(GameState state, ImmutableList<int> cardIndexes)
            : base(state)
        {
            this.CardIndexes = cardIndexes;
        }

        /// <summary>
        /// Gets the indexes of the cards being exchanged.
        /// </summary>
        public ImmutableList<int> CardIndexes { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens =>
            FormatUtilities.ParseStringFormat(Resources.ExchangePetalCardsFormat, FormatUtilities.FormatList(this.GameState.Inventory[this.PlayerToken].Hand));

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is ExchangePetalCardsMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = ValueListComparer<int>.Compare(this.CardIndexes, move.CardIndexes)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Inventory[this.PlayerToken].Hand, move.GameState.Inventory[move.PlayerToken].Hand)) != 0)
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

        internal static IEnumerable<ExchangePetalCardsMove> GenerateMoves(GameState state)
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
            var removed = this.CardIndexes.Select(i => playerInventory.Hand[i]).ToList();

            foreach (var i in this.CardIndexes.OrderByDescending(i => i))
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
