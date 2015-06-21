namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    public class Lamia : Djinn
    {
        public static readonly Lamia Instance = new Lamia();

        private readonly string stateKey;

        private Lamia()
            : base(10)
        {
            this.stateKey = this.GetType().Name + "Used";
        }

        public override IEnumerable<Move> GetAdditionalMoves(GameState state0, IList<Move> moves)
        {
            if (state0.Phase != Phase.End && state0[this.stateKey] == null && state0.Inventory[state0.ActivePlayer].Djinns.Contains(this))
            {
                foreach (var move in moves.OfType<PlacePalmTreeMove>())
                {
                    var newMoves = Cost.OneElderOrOneSlave(state0, s1 => s1.WithState(this.stateKey, "true"), s1 => GetAppliedCostMoves(s1, move));
                    foreach (var m in newMoves) yield return m;
                }
            }
        }

        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            if (oldState.Phase == Phase.CleanUp && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
            {
                newState = newState.WithState(this.stateKey, null);
            }

            return newState;
        }

        private IEnumerable<Move> GetAppliedCostMoves(GameState state0, PlacePalmTreeMove template)
        {
            foreach (var point in Sultanate.GetSquarePoints(template.Point))
            {
                yield return template.With(state: state0, point: point);
            }
        }
    }
}
