// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to zap a card.
    /// </summary>
    public sealed class ZapCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZapCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The location of the card to zap.</param>
        public ZapCardMove(GameState state, Point point)
            : base(state)
        {
            this.Point = point;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.ZapCard, this.GameState.Table[this.Point], this.Point);

        /// <summary>
        /// Gets the location of the card to zap.
        /// </summary>
        public Point Point { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is ZapCardMove zapCard)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.Point.CompareTo(zapCard.Point)) != 0)
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

        /// <inheritdoc />
        internal override GameState Apply(GameState state)
        {
            var playerIndex = state.InventoryMap[this.PlayerToken];
            var playerInventory = state.Inventories[playerIndex];

            playerInventory = playerInventory.With(
                hand: playerInventory.Hand.Add(state.Table[this.Point]));

            state = state.With(
                inventories: state.Inventories.SetItem(
                    playerIndex,
                    playerInventory),
                table: state.Table.Remove(this.Point));

            return base.Apply(state);
        }
    }
}
