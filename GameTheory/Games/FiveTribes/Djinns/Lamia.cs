// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> so that when building a Palm Tree, you may place it on a neighboring Tile instead.
    /// </summary>
    public class Lamia : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Lamia"/>.
        /// </summary>
        public static readonly Lamia Instance = new Lamia();

        private readonly string stateKey;

        private Lamia()
            : base(10)
        {
            this.stateKey = this.GetType().Name + "Used";
        }

        /// <inheritdoc />
        public override IEnumerable<Move> GetAdditionalMoves(GameState state, IList<Move> moves)
        {
            if (state.Phase != Phase.End && state[this.stateKey] == null && state.Inventory[state.ActivePlayer].Djinns.Contains(this))
            {
                foreach (var move in moves.OfType<PlacePalmTreeMove>())
                {
                    var newMoves = Cost.OneElderOrOneSlave(state, s1 => s1.WithState(this.stateKey, "true"), s1 => this.GetAppliedCostMoves(s1, move));

                    foreach (var m in newMoves)
                    {
                        yield return m;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            if (oldState.Phase == Phase.CleanUp && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
            {
                newState = newState.WithState(this.stateKey, null);
            }

            return newState;
        }

        private IEnumerable<Move> GetAppliedCostMoves(GameState state, PlacePalmTreeMove template)
        {
            foreach (var point in Sultanate.GetSquarePoints(template.Point))
            {
                yield return template.With(state, point);
            }
        }
    }
}
