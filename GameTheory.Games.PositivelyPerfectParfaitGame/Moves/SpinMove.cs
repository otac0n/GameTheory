// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to spin the spinner.
    /// </summary>
    public sealed class SpinMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpinMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public SpinMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { Resources.TwirlTheSpinner };

        /// <inheritdoc/>
        public override bool IsDeterministic => false;

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            yield return new SpinMove(state);
        }

        internal override GameState Apply(GameState state)
        {
            return this.GetOutcomes(state).Pick();
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            var activePlayer = this.PlayerToken;

            foreach (var flavor in EnumUtilities<Flavor>.Values)
            {
                var flavorState = state;
                if (state.RemainingScoops[flavor] > 0 && state.Parfaits[activePlayer].Flavors[flavor] == 0)
                {
                    flavorState = state.With(
                        parfaits: state.Parfaits.SetItem(activePlayer, state.Parfaits[activePlayer].With(
                            flavors: state.Parfaits[activePlayer].Flavors.Add(flavor))),
                        remainingScoops: state.RemainingScoops.Remove(flavor));
                }

                yield return Weighted.Create(base.Apply(flavorState), 1);
            }

            var chooseState = state.With(phase: Phase.ChooseAFlavor);

            yield return Weighted.Create(base.Apply(chooseState), 2);

            var oopsState = state;
            if (state.Parfaits[activePlayer].Flavors.Count > 0)
            {
                oopsState = state.With(phase: Phase.Oops);
            }

            yield return Weighted.Create(base.Apply(oopsState), 1);

            var switchState = state;
            if (state.Parfaits[activePlayer].Flavors.Count == 0)
            {
                yield return Weighted.Create(switchState, 1); // `base.Apply` should not be called here, as the turn should not advance when the active player has no scoops to switch.
            }
            else
            {
                if (state.Players.Any(p => p != activePlayer && state.Parfaits[p].Flavors.Count > 0))
                {
                    switchState = state.With(phase: Phase.SwitchAScoop);
                }

                yield return Weighted.Create(base.Apply(switchState), 1);
            }
        }
    }
}
