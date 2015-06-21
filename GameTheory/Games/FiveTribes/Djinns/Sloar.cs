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
                : base(state0, state0.ActivePlayer, s1 =>
                {
                    var player = s1.ActivePlayer;
                    var inventory = s1.Inventory[player];

                    ImmutableList<Resource> dealt;
                    var newDiscards = s1.ResourceDiscards;
                    var newResourcesPile = s1.ResourcePile.Deal(1, out dealt, ref newDiscards);
                    var newInventory = inventory.With(resources: inventory.Resources.AddRange(dealt));

                    return s1.With(
                        inventory: s1.Inventory.SetItem(player, newInventory),
                        resourceDiscards: newDiscards,
                        resourcePile: newResourcesPile);
                })
            {
            }

            public override string ToString()
            {
                return "Draw the top card from the Resource Pile";
            }
        }
    }
}
