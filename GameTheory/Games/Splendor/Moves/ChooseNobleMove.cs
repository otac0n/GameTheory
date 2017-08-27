// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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

        /// <summary>
        /// Gets the noble who would visit.
        /// </summary>
        public Noble Noble => this.State.Nobles[this.Index];

        /// <summary>
        /// Gets the index of the noble in the nobles list.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Invite ", this.Noble, " to visit" };

        internal static bool ShouldTransitionToPhase(GameState state)
        {
            if (state.Phase == Phase.Play || state.Phase == Phase.Discard)
            {
                var bonus = state.GetBonus(state.ActivePlayer);
                return state.Nobles.Any(noble => noble.RequiredBonuses.Keys.All(k => bonus[k] >= noble.RequiredBonuses[k]));
            }

            return false;
        }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
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
