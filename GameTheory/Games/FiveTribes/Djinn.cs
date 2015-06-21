// -----------------------------------------------------------------------
// <copyright file="Djinn.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// The base class for all Djinns.
    /// </summary>
    public abstract class Djinn
    {
        private readonly int value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Djinn"/> class.
        /// </summary>
        /// <param name="value">The value of the Djinn, in victory points (VP).</param>
        protected Djinn(int value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the name of the Djinn.
        /// </summary>
        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
        /// Gets the value of the Djinn, in victory points (VP).
        /// </summary>
        public int Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// Called for every <see cref="GameState"/>, to allow inclusion of additional <see cref="Move">Moves</see>.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> for which additional <see cref="Move">Moves</see> are being generated.</param>
        /// <param name="moves">The standard <see cref="Move">Moves</see> returned by the <see cref="GameState"/>.</param>
        /// <returns>The additional <see cref="Move">Moves</see>.</returns>
        public virtual IEnumerable<Move> GetAdditionalMoves(GameState state0, IList<Move> moves)
        {
            return ImmutableList<Move>.Empty;
        }

        /// <summary>
        /// Generate moves for any <see cref="GameState"/> where this <see cref="Djinn"/> is owned by the active player.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> for which <see cref="Move">Moves</see> are being generated.</param>
        /// <returns>The <see cref="Move">Moves</see> provided by the <see cref="Djinn"/>.</returns>
        public virtual IEnumerable<Move> GetMoves(GameState state)
        {
            return ImmutableList<Move>.Empty;
        }

        public virtual GameState HandleAssassination(PlayerToken owner, GameState state0, PlayerToken victim, EnumCollection<Meeple> kill)
        {
            return state0;
        }

        public virtual GameState HandleAssassination(PlayerToken owner, GameState state0, Point point, EnumCollection<Meeple> kill)
        {
            return state0;
        }

        public virtual GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            return newState;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public abstract class OnAcquireDjinnBase : Djinn
        {
            protected OnAcquireDjinnBase(int value)
                : base(value)
            {
            }

            public sealed override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
            {
                if (!oldState.Inventory[owner].Djinns.Contains(this))
                {
                    return this.OnAcquire(owner, newState);
                }
                else
                {
                    return newState;
                }
            }

            protected virtual GameState OnAcquire(PlayerToken owner, GameState state)
            {
                return state;
            }
        }

        public abstract class PayPerActionDjinnBase : Djinn
        {
            private readonly CostDelegate cost;
            private readonly string stateKey;

            protected PayPerActionDjinnBase(int value, CostDelegate cost)
                : base(value)
            {
                this.cost = cost;
                this.stateKey = this.GetType().Name + "Used";
            }

            public sealed override IEnumerable<Move> GetMoves(GameState state0)
            {
                if (this.CanGetMoves(state0))
                {
                    return this.cost(state0, s1 => s1.WithState(this.stateKey, "true"), this.GetAppliedCostMoves);
                }

                return base.GetMoves(state0);
            }

            public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
            {
                if (oldState.Phase == Phase.CleanUp && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
                {
                    newState = this.CleanUp(newState.WithState(this.stateKey, null));
                }

                return newState;
            }

            protected virtual bool CanGetMoves(GameState state)
            {
                if (state[this.stateKey] == null)
                {
                    switch (state.Phase)
                    {
                        case Phase.PickUpMeeples:
                        case Phase.MoveMeeples:
                        case Phase.TileControlCheck:
                        case Phase.TribesAction:
                        case Phase.TileAction:
                        case Phase.CleanUp:
                            return state.Inventory[state.ActivePlayer].Djinns.Contains(this);
                    }
                }

                return false;
            }

            protected virtual GameState CleanUp(GameState state)
            {
                return state;
            }

            protected abstract IEnumerable<Move> GetAppliedCostMoves(GameState state);
        }
    }
}
