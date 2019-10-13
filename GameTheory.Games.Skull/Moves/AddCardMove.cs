// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to add a card to the player's stack.
    /// </summary>
    public sealed class AddCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="card">The card to add.</param>
        public AddCardMove(GameState state, Card card)
            : base(state)
        {
            this.Card = card;
        }

        /// <summary>
        /// Gets the card to add.
        /// </summary>
        public Card Card { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.AddACard, this.Card);

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is AddCardMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = EnumComparer<Card>.Default.Compare(this.Card, move.Card)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Players, move.GameState.Players)) != 0)
                {
                    return comp;
                }

                foreach (var player in this.GameState.Players)
                {
                    var thisInventory = this.GameState.Inventory[player];
                    var moveInventory = move.GameState.Inventory[player];
                    if ((comp = thisInventory.Stack.Count.CompareTo(moveInventory.Stack.Count)) != 0)
                    {
                        return comp;
                    }
                }

                return 0;
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<AddCardMove> GenerateMoves(GameState state)
        {
            foreach (var card in state.Inventory[state.ActivePlayer].Hand.Keys)
            {
                yield return new AddCardMove(state, card);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory.With(
                        hand: playerInventory.Hand.Remove(this.Card),
                        stack: playerInventory.Stack.Add(this.Card))));

            return base.Apply(state);
        }
    }
}
