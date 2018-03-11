// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to pass a challenge.
    /// </summary>
    public sealed class PassMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PassMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public PassMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Pass" };

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is BidMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Players, move.GameState.Players)) != 0)
                {
                    return comp;
                }

                foreach (var player in this.GameState.Players)
                {
                    var thisInventory = this.GameState.Inventory[player];
                    var moveInventory = move.GameState.Inventory[player];
                    if ((comp = thisInventory.Bid.CompareTo(moveInventory.Bid)) != 0 &&
                        (comp = thisInventory.Stack.Count.CompareTo(moveInventory.Stack.Count)) != 0)
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

        internal static IEnumerable<PassMove> GenerateMoves(GameState state)
        {
            yield return new PassMove(state);
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory.With(
                        bid: Inventory.PassingBid)));

            return base.Apply(state);
        }
    }
}
