// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides the base class for interstitial states.
    /// </summary>
    public abstract class InterstitialState : IComparable<InterstitialState>
    {
        /// <inheritdoc />
        public virtual int CompareTo(InterstitialState other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            return string.Compare(this.GetType().FullName, other.GetType().FullName, StringComparison.Ordinal);
        }

        /// <summary>
        /// Enumerates the moves available in this state.
        /// </summary>
        /// <param name="state">The starting game state.</param>
        /// <returns>An enumerable collection of moves.</returns>
        public abstract IEnumerable<Move> GenerateMoves(GameState state);
    }
}
