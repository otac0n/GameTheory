// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to arrange cards for another player to choose from.
    /// </summary>
    public sealed class ArrangeCardsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrangeCardsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="arrangement">The arrangement the player will present.</param>
        public ArrangeCardsMove(GameState state, ImmutableList<Card> arrangement)
            : base(state)
        {
            this.Arrangement = arrangement;
        }

        /// <summary>
        /// Gets the arrangement the player will present.
        /// </summary>
        public ImmutableList<Card> Arrangement { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.PresentCards, FormatUtilities.FormatList(this.Arrangement));

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is ArrangeCardsMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = CompareUtilities.CompareEnumLists(this.Arrangement, move.Arrangement)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Players, move.GameState.Players)) != 0)
                {
                    return comp;
                }

                var thisChooser = this.GameState.Players.First(p => this.GameState.Inventory[p].Revealed[Card.Skull] > 0);
                var moveChooser = move.GameState.Players.First(p => move.GameState.Inventory[p].Revealed[Card.Skull] > 0);

                return thisChooser.CompareTo(moveChooser);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<ArrangeCardsMove> GenerateMoves(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var activePlayerInventory = state.Inventory[activePlayer];
            if (activePlayerInventory.Revealed.Count > 0 && activePlayerInventory.Revealed[Card.Skull] == 0)
            {
                var cards = activePlayerInventory.Hand.AddRange(activePlayerInventory.Revealed);
                if (cards[Card.Skull] > 0)
                {
                    var arrangement = ImmutableList.CreateRange(cards.RemoveAll(Card.Skull));

                    for (var i = 0; i <= arrangement.Count; i++)
                    {
                        yield return new ArrangeCardsMove(state, arrangement.Insert(i, Card.Skull));
                    }
                }
                else
                {
                    yield return new ArrangeCardsMove(state, cards.ToImmutableList());
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[state.ActivePlayer];

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory.With(
                        presentedCards: this.Arrangement,
                        hand: EnumCollection<Card>.Empty,
                        revealed: EnumCollection<Card>.Empty)));

            return base.Apply(state);
        }
    }
}
