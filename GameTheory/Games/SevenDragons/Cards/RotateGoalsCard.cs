// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Cards
{
    using System.Collections.Generic;
    using GameTheory.Games.SevenDragons.Moves;

    /// <summary>
    /// Represents a Rotate Goals card.
    /// </summary>
    public sealed class RotateGoalsCard : ActionCard
    {
        /// <summary>
        /// The singleton instance of the <see cref="RotateGoalsCard"/> class.
        /// </summary>
        public static readonly RotateGoalsCard Instance = new RotateGoalsCard();

        private RotateGoalsCard()
            : base(Color.Blue)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new[] { Resources.RotateGoals };

        /// <inheritdoc />
        internal override IEnumerable<Move> GenerateActionMoves(GameState state)
        {
            yield return new RotateGoalsMove(state, RotateDirection.AlongTurnOrder);
            yield return new RotateGoalsMove(state, RotateDirection.OppositeTurnOrder);
        }
    }
}
