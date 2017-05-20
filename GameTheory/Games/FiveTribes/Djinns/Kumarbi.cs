// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneOrMoreSlaves"/>: for each Slave you discard your bidding cost is reduced by 1 spot.
    /// </summary>
    public class Kumarbi : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Kumarbi"/>.
        /// </summary>
        public static readonly Kumarbi Instance = new Kumarbi();

        private readonly string stateKey;

        private Kumarbi()
            : base(6)
        {
            this.stateKey = this.GetType().Name + "Used";
        }

        /// <inheritdoc />
        public sealed override IEnumerable<Move> GetMoves(GameState state)
        {
            if (this.CanGetMoves(state))
            {
                return Cost.OneOrMoreSlaves(state, s1 => s1.WithState(this.stateKey, "true"), this.GetAppliedCostMoves);
            }

            return base.GetMoves(state);
        }

        /// <inheritdoc />
        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            if (oldState == null)
            {
                throw new ArgumentNullException(nameof(oldState));
            }

            if (newState == null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            if (oldState.Phase == Phase.CleanUp && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
            {
                newState = newState.WithState(this.stateKey, null);
            }

            return newState;
        }

        private bool CanGetMoves(GameState state)
        {
            if (state[this.stateKey] == null && state.Phase == Phase.Bid)
            {
                return state.Inventory[state.ActivePlayer].Djinns.Contains(this);
            }

            return false;
        }

        private IEnumerable<Move> GetAppliedCostMoves(GameState state, int slaves)
        {
            if (slaves > state.TurnOrderTrack.LastIndexOf(null) - 2)
            {
                yield break;
            }

            var turnOrderTrackCosts = GameState.TurnOrderTrackCosts.InsertRange(0, new int[slaves]);

            for (var i = 2; i < state.TurnOrderTrack.Count; i++)
            {
                if (state.TurnOrderTrack[i] == null && state.Inventory[state.ActivePlayer].GoldCoins >= turnOrderTrackCosts[i])
                {
                    var j = i == 2 && state.TurnOrderTrack[0] == null ? 0 :
                            i == 2 && state.TurnOrderTrack[1] == null ? 1 :
                            i;

                    yield return new BidMove(state, j, turnOrderTrackCosts[j]);
                }
            }
        }
    }
}
