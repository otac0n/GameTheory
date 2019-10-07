// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Moves
{
    using System.Collections.Generic;
    using GameTheory.Games.SevenDragons.Cards;

    /// <summary>
    /// Represents a move to move a dragon card on the table.
    /// </summary>
    public sealed class MoveCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoveCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="fromPoint">The point from which the card will be moved.</param>
        /// <param name="toPoint">The point to which the card will be played.</param>
        /// <param name="orientation">The orientation of the card being played.</param>
        public MoveCardMove(GameState state, Point fromPoint, Point toPoint, DragonCard orientation)
            : base(state)
        {
            this.FromPoint = fromPoint;
            this.ToPoint = toPoint;
            this.Orientation = orientation;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.MoveCard, this.Orientation, this.FromPoint, this.ToPoint);

        /// <summary>
        /// Gets the point from which the card will be moved.
        /// </summary>
        public Point FromPoint { get; }

        /// <summary>
        /// Gets the orientation of the card being played.
        /// </summary>
        public DragonCard Orientation { get; }

        /// <summary>
        /// Gets the point to which the card will be played.
        /// </summary>
        public Point ToPoint { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is MoveCardMove moveCard)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.FromPoint.CompareTo(moveCard.FromPoint)) != 0 ||
                    (comp = this.ToPoint.CompareTo(moveCard.ToPoint)) != 0 ||
                    (comp = this.Orientation.CompareTo(moveCard.Orientation)) != 0)
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
            state = state.With(
                table: state.Table.Remove(this.FromPoint).Add(this.ToPoint, this.Orientation));

            return base.Apply(state);
        }
    }
}
