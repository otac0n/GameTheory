// -----------------------------------------------------------------------
// <copyright file="Move.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System.Diagnostics.Contracts;

    public abstract class Move : IMove
    {
        protected Move(GameState state, PlayerToken player)
        {
            Contract.Requires(player != null);

            this.State = state;
            this.Player = player;
        }

        public PlayerToken Player { get; private set; }

        internal GameState State { get; private set; }

        public abstract override string ToString();

        internal abstract GameState Apply(GameState state0);
    }
}
