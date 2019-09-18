// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Cards
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.SevenDragons.Moves;

    /// <summary>
    /// Represents a Move a Card card.
    /// </summary>
    public sealed class MoveACardCard : ActionCard
    {
        /// <summary>
        /// The singleton instance of the <see cref="MoveACardCard"/> class.
        /// </summary>
        public static readonly MoveACardCard Instance = new MoveACardCard();

        private MoveACardCard()
            : base(Color.Green)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new[] { Resources.MoveACard };

        /// <inheritdoc />
        internal override IEnumerable<Move> GenerateActionMoves(GameState state)
        {
            foreach (var key in state.Table.Keys)
            {
                if (key.X != 0 || key.Y != 0)
                {
                    var card = state.Table[key];
                    var stateWithoutCard = state.With(table: state.Table.Remove(key));
                    var adjacent = stateWithoutCard.GetEmptyAdjacent();
                    var orientations = card == card.Reversed
                        ? new[] { card }
                        : new[] { card, card.Reversed };
                    foreach (var orientation in orientations)
                    {
                        foreach (var point in adjacent)
                        {
                            if (Enumerable.Range(0, DragonCard.Grid.Count).Any(j =>
                            {
                                var color = orientation.Colors[j];
                                return color == Color.Rainbow || state.GetMatchingAdjacent(point, DragonCard.Grid[j], color, color).Any();
                            }))
                            {
                                yield return new MoveCardMove(state, key, point, orientation);
                            }
                        }
                    }
                }
            }
        }
    }
}
