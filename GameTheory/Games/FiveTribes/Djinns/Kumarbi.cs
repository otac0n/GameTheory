// -----------------------------------------------------------------------
// <copyright file="Kumarbi.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using GameTheory.Games.FiveTribes.Moves;

    public class Kumarbi : Djinn
    {
        public static readonly Kumarbi Instance = new Kumarbi();

        private readonly string stateKey;

        protected Kumarbi()
            : base(6)
        {
            this.stateKey = this.GetType().Name + "Used";
        }

        public sealed override IEnumerable<Move> GetMoves(GameState state0)
        {
            if (this.CanGetMoves(state0))
            {
                return Cost.OneOrMoreSlaves(state0, s1 => s1.WithState(this.stateKey, "true"), this.GetAppliedCostMoves);
            }

            return base.GetMoves(state0);
        }

        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            if (oldState.Phase == Phase.CleanUp && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
            {
                newState = newState.WithState(this.stateKey, null);
            }

            return newState;
        }

        protected virtual bool CanGetMoves(GameState state)
        {
            if (state[this.stateKey] == null && state.Phase == Phase.Bid)
            {
                return state.Inventory[state.ActivePlayer].Djinns.Contains(this);
            }

            return false;
        }

        private IEnumerable<Move> GetAppliedCostMoves(GameState state0, int slaves)
        {
            if (slaves > state0.TurnOrderTrack.LastIndexOf(null) - 2)
            {
                yield break;
            }

            var turnOrderTrackCosts = GameState.TurnOrderTrackCosts.InsertRange(0, new int[slaves]);

            for (var i = 2; i < state0.TurnOrderTrack.Count; i++)
            {
                if (state0.TurnOrderTrack[i] == null && state0.Inventory[state0.ActivePlayer].GoldCoins >= turnOrderTrackCosts[i])
                {
                    var j = i == 2 && state0.TurnOrderTrack[0] == null ? 0 :
                            i == 2 && state0.TurnOrderTrack[1] == null ? 1 :
                            i;

                    yield return new BidMove(state0, j, turnOrderTrackCosts[j]);
                }
            }
        }
    }
}
