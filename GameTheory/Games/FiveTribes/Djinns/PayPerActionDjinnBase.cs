// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Encapsulates <see cref="Djinn"/> behaviors that are activated with a <see cref="Cost"/>.
    /// </summary>
    public abstract class PayPerActionDjinnBase : Djinn
    {
        private readonly ApplyCost cost;
        private readonly string stateKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayPerActionDjinnBase"/> class.
        /// </summary>
        /// <param name="value">The value of the <see cref="Djinn"/>, in victory points (VP).</param>
        /// <param name="cost">The <see cref="Cost"/> of the ability.</param>
        protected PayPerActionDjinnBase(int value, ApplyCost cost)
            : base(value)
        {
            this.cost = cost;
            this.stateKey = this.GetType().Name + "Used";
        }

        /// <inheritdoc />
        public sealed override IEnumerable<Move> GetMoves(GameState state)
        {
            if (this.CanGetMoves(state))
            {
                return this.cost(state, s1 => s1.WithState(this.stateKey, "true").WithInterstitialState(this.GetInterstitialState()));
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

            if (oldState.Phase == Phase.MerchandiseSale && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
            {
                newState = this.CleanUp(newState.WithState(this.stateKey, null));
            }

            return newState;
        }

        /// <summary>
        /// Gets a value indicating whether or not the <see cref="Djinn"/> can activate its ability in the specified <see cref="GameState"/>.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> to evaluate.</param>
        /// <returns><c>true</c>, if the ability can be activated, <c>false</c> otherwise.</returns>
        protected virtual bool CanGetMoves(GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state[this.stateKey] == null)
            {
                switch (state.Phase)
                {
                    case Phase.PickUpMeeples:
                    case Phase.MoveMeeples:
                    case Phase.TileControlCheck:
                    case Phase.TribesAction:
                    case Phase.TileAction:
                    case Phase.MerchandiseSale:
                        return state.Inventory[state.ActivePlayer].Djinns.Contains(this);
                }
            }

            return false;
        }

        /// <summary>
        /// Perform clean up.
        /// </summary>
        /// <param name="state">The current <see cref="GameState"/>.</param>
        /// <returns>An updated <see cref="GameState"/>.</returns>
        protected virtual GameState CleanUp(GameState state)
        {
            return state;
        }

        /// <summary>
        /// Generate the <see cref="InterstitialState"/> after the <see cref="Cost"/> has been paid.
        /// </summary>
        /// <returns>The <see cref="InterstitialState"/> provided by the <see cref="Djinn"/>.</returns>
        protected abstract InterstitialState GetInterstitialState();
    }
}
