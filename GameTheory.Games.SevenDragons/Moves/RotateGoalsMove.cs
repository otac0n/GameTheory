// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to rotate all player goals.
    /// </summary>
    public sealed class RotateGoalsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RotateGoalsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="direction">The rotation direction.</param>
        public RotateGoalsMove(GameState state, RotateDirection direction)
            : base(state)
        {
            this.Direction = direction;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => this.Direction == RotateDirection.AlongTurnOrder
            ? new[] { Resources.RotateAlongTurnOrder }
            : new[] { Resources.RotateOppositeTurnOrder };

        /// <summary>
        /// Gets the rotation direction.
        /// </summary>
        public RotateDirection Direction { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is RotateGoalsMove rotateGoals)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.Direction.CompareTo(rotateGoals.Direction)) != 0)
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
            var offset = this.Direction == RotateDirection.AlongTurnOrder
                ? 1
                : (state.Inventories.Count - 1);

            var inventories = state.Inventories;
            for (var fromIndex = 0; fromIndex < state.Inventories.Count; fromIndex++)
            {
                var toIndex = (fromIndex + offset) % state.Inventories.Count;

                inventories = inventories.SetItem(toIndex, state.Inventories[toIndex].With(goal: state.Inventories[fromIndex].Goal));
            }

            state = state.With(
                inventories: inventories);

            return base.Apply(state);
        }
    }
}
