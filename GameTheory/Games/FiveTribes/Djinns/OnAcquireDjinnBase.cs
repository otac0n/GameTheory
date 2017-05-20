// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;

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
            if (oldState == null)
            {
                throw new ArgumentNullException(nameof(oldState));
            }

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
}
