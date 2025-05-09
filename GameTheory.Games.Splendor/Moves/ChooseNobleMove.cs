﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to choose a noble to visit.
    /// </summary>
    public sealed class ChooseNobleMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChooseNobleMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the noble who would visit.</param>
        public ChooseNobleMove(GameState state, int index)
            : base(state)
        {
            this.Index = index;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.ChooseNoble, this.Noble);

        /// <summary>
        /// Gets the index of the noble in the nobles list.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the noble who would visit.
        /// </summary>
        public Noble Noble => this.GameState.Nobles[this.Index];

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is ChooseNobleMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.Index.CompareTo(move.Index)) != 0 ||
                    (comp = this.GameState.Nobles[this.Index].CompareTo(move.GameState.Nobles[move.Index])) != 0)
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

        internal static IEnumerable<ChooseNobleMove> GenerateMoves(GameState state)
        {
            var bonus = state.GetBonus(state.ActivePlayer);
            for (var i = 0; i < state.Nobles.Count; i++)
            {
                var noble = state.Nobles[i];
                if (noble.RequiredBonuses.Keys.All(k => bonus[k] >= noble.RequiredBonuses[k]))
                {
                    yield return new ChooseNobleMove(state, i);
                }
            }
        }

        internal static bool ShouldTransitionToPhase(GameState state)
        {
            if (state.Phase == Phase.Play || state.Phase == Phase.Discard)
            {
                var bonus = state.GetBonus(state.ActivePlayer);
                return state.Nobles.Any(noble => noble.RequiredBonuses.Keys.All(k => bonus[k] >= noble.RequiredBonuses[k]));
            }

            return false;
        }

        internal override GameState Apply(GameState state)
        {
            var nobles = state.Nobles;
            var pInventory = state.Inventory[state.ActivePlayer];
            var pNobles = pInventory.Nobles;

            pNobles = pNobles.Add(nobles[this.Index]);
            nobles = nobles.RemoveAt(this.Index);

            return base.Apply(state.With(
                nobles: nobles,
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    nobles: pNobles))));
        }
    }
}
