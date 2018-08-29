// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to take a cherry.
    /// </summary>
    public sealed class TakeACherryMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TakeACherryMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public TakeACherryMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { Resources.TakeACherry };

        /// <inheritdoc/>
        public override bool IsDeterministic => true;

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            if (state.Parfaits[state.ActivePlayer].Flavors.Keys.Count() >= 3)
            {
                yield return new TakeACherryMove(state);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var activePlayer = this.PlayerToken;

            return base.Apply(state.With(
                phase: Phase.Play,
                parfaits: state.Parfaits.SetItem(activePlayer, state.Parfaits[activePlayer].With(cherry: true))));
        }
    }
}
