namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Diagnostics.Contracts;

    public abstract class Move : IMove
    {
        protected Move(GameState state, PlayerToken player, Func<GameState, GameState> apply)
        {
            Contract.Requires(player != null);

            this.State = state;
            this.Player = player;
            this.Apply = apply;
        }

        public PlayerToken Player { get; private set; }

        internal Func<GameState, GameState> Apply { get; private set; }

        internal GameState State { get; private set; }

        public abstract override string ToString();
    }
}
