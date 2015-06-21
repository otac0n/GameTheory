namespace GameTheory.Games.FiveTribes
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public abstract class Djinn
    {
        private readonly int value;

        protected Djinn(int value)
        {
            this.value = value;
        }

        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        public int Value
        {
            get { return this.value; }
        }

        public virtual IEnumerable<Move> GetAdditionalMoves(GameState state0, IList<Move> moves)
        {
            return ImmutableList<Move>.Empty;
        }

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
                    return this.cost(state0, s1 => s1.WithState(this.stateKey, "true"), GetAppliedCostMoves);
                }

                return base.GetMoves(state0);
            }

            public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
            {
                if (oldState.Phase == Phase.CleanUp && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
                {
                    newState = CleanUp(newState.WithState(this.stateKey, null));
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
