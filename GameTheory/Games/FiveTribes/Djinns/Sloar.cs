// -----------------------------------------------------------------------
// <copyright file="Sloar.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public class Sloar : Djinn.PayPerActionDjinnBase
    {
        public static readonly Sloar Instance = new Sloar();

        private Sloar()
            : base(8, Cost.OneSlave)
        {
        }

        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            yield return new DrawTopCardMove(state0);
        }

        public class DrawTopCardMove : Move
        {
            public DrawTopCardMove(GameState state0)
                : base(state0, state0.ActivePlayer)
            {
            }

            public override string ToString()
            {
                return "Draw the top card from the Resource Pile";
            }

            internal override GameState Apply(GameState state0)
            {
                var player = state0.ActivePlayer;
                var inventory = state0.Inventory[player];

                ImmutableList<Resource> dealt;
                var newDiscards = state0.ResourceDiscards;
                var newResourcesPile = state0.ResourcePile.Deal(1, out dealt, ref newDiscards);
                var newInventory = inventory.With(resources: inventory.Resources.AddRange(dealt));

                return state0.With(
                    inventory: state0.Inventory.SetItem(player, newInventory),
                    resourceDiscards: newDiscards,
                    resourcePile: newResourcesPile);
            }
        }
    }
}
