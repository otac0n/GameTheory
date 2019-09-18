// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to choose a flavor from the remaining scoops.
    /// </summary>
    public sealed class ChooseAFlavorMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChooseAFlavorMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="flavor">The flavor chosen.</param>
        public ChooseAFlavorMove(GameState state, Flavor flavor)
            : base(state)
        {
            this.Flavor = flavor;
        }

        /// <summary>
        /// Gets the flavor chosen.
        /// </summary>
        public Flavor Flavor { get; }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.TakeAScoop, this.Flavor);

        /// <inheritdoc/>
        public override bool IsDeterministic => true;

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            foreach (var flavor in state.RemainingScoops.Keys)
            {
                yield return new ChooseAFlavorMove(state, flavor);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var activePlayer = this.PlayerToken;

            return base.Apply(state.With(
                phase: Phase.Play,
                parfaits: state.Parfaits.SetItem(activePlayer, state.Parfaits[activePlayer].With(
                    flavors: state.Parfaits[activePlayer].Flavors.Add(this.Flavor))),
                remainingScoops: state.RemainingScoops.Remove(this.Flavor)));
        }
    }
}
