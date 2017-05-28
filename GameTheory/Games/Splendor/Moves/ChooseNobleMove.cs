// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to choose a noble to visit.
    /// </summary>
    public class ChooseNobleMove : Move
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
        public override string ToString() => $"Invite {this.Noble} to visit.";

        internal static GameState GenerateTransitionState(GameState state)
        {
            var bonus = state.GetBonus(state.ActivePlayer);
            if (state.Nobles.Any(noble => noble.RequiredBonuses.Keys.All(k => bonus[k] >= noble.RequiredBonuses[k])))
            {
                return state.WithMoves(newState =>
                {
                    var builder = ImmutableList.CreateBuilder<Move>();

                    for (int i = 0; i < newState.Nobles.Count; i++)
                    {
                        var noble = newState.Nobles[i];
                        if (noble.RequiredBonuses.Keys.All(k => bonus[k] >= noble.RequiredBonuses[k]))
                        {
                             builder.Add(new ChooseNobleMove(newState, i));
                        }
                    }

                    return builder.ToImmutable();
                });
            }
            else
            {
                return state;
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
