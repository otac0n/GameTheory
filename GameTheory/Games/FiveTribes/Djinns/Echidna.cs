namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;

    public class Echidna : Djinn.PayPerActionDjinnBase
    {
        public static readonly Echidna Instance = new Echidna();

        private Echidna()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        protected override GameState CleanUp(GameState state)
        {
            var player = state.Inventory.Where(i => i.Value.Djinns.Contains(this)).Select(i => i.Key).Single();
            var scoreTable = state.ScoreTables[player];

            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, scoreTable.With(builderMultiplier: scoreTable.BuilderMultiplier / 2)));
        }

        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            yield return new DoubleBuilderScoreMove(state0);
        }

        public class DoubleBuilderScoreMove : Move
        {
            public DoubleBuilderScoreMove(GameState state0)
                : base(state0, state0.ActivePlayer, s1 =>
                {
                    var player = s1.ActivePlayer;
                    var scoreTable = s1.ScoreTables[player];

                    return s1.With(
                        scoreTables: s1.ScoreTables.SetItem(player, scoreTable.With(builderMultiplier: scoreTable.BuilderMultiplier * 2)));
                })
            {
            }

            public override string ToString()
            {
                return "Double the amout of GCs your Builders get this turn";
            }
        }
    }
}
