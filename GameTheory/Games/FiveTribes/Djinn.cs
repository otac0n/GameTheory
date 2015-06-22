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
        /// <param name="value">The value of the <see cref="Djinn"/>, in victory points (VP).</param>
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
        public virtual IEnumerable<Move> GetMoves(GameState state0)
        {
            return ImmutableList<Move>.Empty;
        }

        /// <summary>
        /// Handle an assassination in front of a player.
        /// </summary>
        /// <param name="owner">The owner of the <see cref="Djinn"/>.</param>
        /// <param name="state0">The <see cref="GameState"/> that has recently seen an assassination.</param>
        /// <param name="victim">The victim of the assassination.</param>
        /// <param name="kill">The <see cref="Meeple">Meeples</see> killed.</param>
        /// <returns>An updated <see cref="GameState"/>.</returns>
        public virtual GameState HandleAssassination(PlayerToken owner, GameState state0, PlayerToken victim, EnumCollection<Meeple> kill)
        {
            return state0;
        }

        /// <summary>
        /// Handle an assassination in the <see cref="Sultanate"/>.
        /// </summary>
        /// <param name="owner">The owner of the <see cref="Djinn"/>.</param>
        /// <param name="state0">The <see cref="GameState"/> that has recently seen an assassination.</param>
        /// <param name="point">The location of the assassination.</param>
        /// <param name="kill">The <see cref="Meeple">Meeples</see> killed.</param>
        /// <returns>An updated <see cref="GameState"/>.</returns>
        public virtual GameState HandleAssassination(PlayerToken owner, GameState state0, Point point, EnumCollection<Meeple> kill)
        {
            return state0;
        }

        /// <summary>
        /// Handle the transition between states.
        /// </summary>
        /// <param name="owner">The owner of the <see cref="Djinn"/>.</param>
        /// <param name="oldState">The oldest <see cref="GameState"/> that has been handled by this Djinn.</param>
        /// <param name="newState">The newest <see cref="GameState"/>.</param>
        /// <returns>An updated <see cref="GameState"/>.</returns>
        public virtual GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            return newState;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Encapsulates <see cref="Djinn"/> behaviors that happen when the <see cref="Djinn"/> is acquired.
        /// </summary>
        public abstract class OnAcquireDjinnBase : Djinn
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OnAcquireDjinnBase"/> class.
            /// </summary>
            /// <param name="value">The value of the <see cref="Djinn"/>, in victory points (VP).</param>
            protected OnAcquireDjinnBase(int value)
                : base(value)
            {
            }

            /// <inheritdoc />
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

            /// <summary>
            /// Called when the <see cref="Djinn"/> is acquired.
            /// </summary>
            /// <param name="owner">The owner of the <see cref="Djinn"/>.</param>
            /// <param name="state">The <see cref="GameState"/> when the <see cref="Djinn"/> is acquired.</param>
            /// <returns>An updated <see cref="GameState"/>.</returns>
            protected virtual GameState OnAcquire(PlayerToken owner, GameState state)
            {
                return state;
            }
        }

        /// <summary>
        /// Encapsulates <see cref="Djinn"/> behaviors that are activated with a <see cref="Cost"/>.
        /// </summary>
        public abstract class PayPerActionDjinnBase : Djinn
        {
            private readonly CostDelegate cost;
            private readonly string stateKey;

            /// <summary>
            /// Initializes a new instance of the <see cref="PayPerActionDjinnBase"/> class.
            /// </summary>
            /// <param name="value">The value of the <see cref="Djinn"/>, in victory points (VP).</param>
            /// <param name="cost">The <see cref="Cost"/> of the ability.</param>
            protected PayPerActionDjinnBase(int value, CostDelegate cost)
                : base(value)
            {
                this.cost = cost;
                this.stateKey = this.GetType().Name + "Used";
            }

            /// <inheritdoc />
            public sealed override IEnumerable<Move> GetMoves(GameState state0)
            {
                if (this.CanGetMoves(state0))
                {
                    return this.cost(state0, s1 => s1.WithState(this.stateKey, "true"), this.GetAppliedCostMoves);
                }

                return base.GetMoves(state0);
            }

            /// <inheritdoc />
            public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
            {
                if (oldState.Phase == Phase.CleanUp && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
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
            /// Generate the subsequent moves after the <see cref="Cost"/> has been paid.
            /// </summary>
            /// <param name="state">The <see cref="GameState"/> after the <see cref="Cost"/> has been applied.</param>
            /// <returns>The <see cref="Move">Moves</see> provided by the <see cref="Djinn"/>.</returns>
            protected abstract IEnumerable<Move> GetAppliedCostMoves(GameState state);
        }
    }
}
