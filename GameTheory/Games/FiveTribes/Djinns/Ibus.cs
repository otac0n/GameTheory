namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;

    public class Ibus : Djinn.PayPerActionDjinnBase
    {
        public static readonly Ibus Instance = new Ibus();

        private Ibus()
            : base(8, Cost.OneElderOrOneSlave)
        {
        }

        protected override GameState CleanUp(GameState state)
        {
            var player = state.Inventory.Where(i => i.Value.Djinns.Contains(this)).Select(i => i.Key).Single();
            var assassinationTable = state.AssassinationTables[player];

            return state.With(
                assassinationTables: state.AssassinationTables.SetItem(player, assassinationTable.With(killCount: 1)));
        }

        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            yield return new DoubleAssassinKillCountMove(state0);
        }

        public class DoubleAssassinKillCountMove : Move
        {
            public DoubleAssassinKillCountMove(GameState state0)
                : base(state0, state0.ActivePlayer, s1 =>
                {
                    var player = s1.ActivePlayer;
                    var assassinationTable = s1.AssassinationTables[player];

                    return s1.With(
                        assassinationTables: s1.AssassinationTables.SetItem(player, assassinationTable.With(killCount: 2)));
                })
            {
            }

            public override string ToString()
            {
                return "Double the number of meeples your Assassins kill this turn";
            }
        }
    }
}
