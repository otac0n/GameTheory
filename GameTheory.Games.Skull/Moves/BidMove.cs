// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to issue a challenge.
    /// </summary>
    public sealed class BidMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BidMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="bid">The player's bid.</param>
        public BidMove(GameState state, int bid)
            : base(state)
        {
            this.Bid = bid;
        }

        /// <summary>
        /// Gets the player's bid.
        /// </summary>
        public int Bid { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.Bid, this.Bid);

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is BidMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.Bid.CompareTo(move.Bid)) != 0 ||
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

        internal static IEnumerable<BidMove> GenerateMoves(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var activePlayerInventory = state.Inventory[activePlayer];
            if (activePlayerInventory.Stack.Count >= 1)
            {
                var totalCards = state.Inventory.Values.Sum(i => i.Stack.Count);
                var minBid = state.Inventory.Values.Max(i => i.Bid) + 1;
                for (var bid = minBid; bid <= totalCards; bid++)
                {
                    yield return new BidMove(state, bid);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];
            var totalCards = state.Inventory.Values.Sum(i => i.Stack.Count);

            var phase = this.Bid == totalCards
                ? Phase.Challenge
                : Phase.Bidding;

            state = state.With(
                phase: phase,
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory.With(
                        bid: this.Bid)));

            return base.Apply(state);
        }
    }
}
