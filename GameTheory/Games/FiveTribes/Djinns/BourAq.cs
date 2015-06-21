namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    public class Bouraq : Djinn.PayPerActionDjinnBase
    {
        public static readonly Bouraq Instance = new Bouraq();

        private Bouraq()
            : base(6, Cost.OneElderOrOneSlave)
        {
        }

        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            return Enumerable.Range(0, Sultanate.Width * Sultanate.Height)
                .Where(i => state0.Sultanate[i].Tile is Tile.Village)
                .Select(i => new PlacePalaceMove(state0, i));
        }
    }
}
