// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> so that when placing a Palace, you may drop it on any neighboring Tile instead.
    /// </summary>
    public class Hagis : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Hagis"/>.
        /// </summary>
        public static readonly Hagis Instance = new Hagis();

        private readonly string stateKey;

        private Hagis()
            : base(10)
        {
            this.stateKey = this.GetType().Name + "Used";
        }

        /// <inheritdoc />
        public override IEnumerable<Move> GetAdditionalMoves(GameState state, IList<Move> moves)
        {
            if (state.Phase != Phase.End && state[this.stateKey] == null && state.Inventory[state.ActivePlayer].Djinns.Contains(this))
            {
                foreach (var move in moves.OfType<PlacePalaceMove>())
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
            Contract.Requires(oldState != null);
            Contract.Requires(newState != null);

            if (oldState.Phase == Phase.CleanUp && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
            {
                newState = newState.WithState(this.stateKey, null);
            }

            return newState;
        }

        private IEnumerable<Move> GetAppliedCostMoves(GameState state, PlacePalaceMove template)
        {
            foreach (var point in Sultanate.GetSquarePoints(template.Point))
            {
                yield return template.With(state, point);
            }
        }
    }
}
