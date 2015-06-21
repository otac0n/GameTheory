namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    public class Enki : Djinn.PayPerActionDjinnBase
    {
        public static readonly Enki Instance = new Enki();

        private Enki()
            : base(8, Cost.OneElderOrOneSlave)
        {
        }

        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            return Enumerable.Range(0, Sultanate.Width * Sultanate.Height)
                .Where(i => state0.Sultanate[i].Tile is Tile.Oasis)
                .Select(i => new PlacePalmTreeMove(state0, i));
        }
    }
}
