namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AlAmin : Djinn
    {
        public static readonly AlAmin Instance = new AlAmin();

        private AlAmin()
            : base(5)
        {
        }

        public override string Name
        {
            get { return "Al-Amin"; }
        }

        public override IEnumerable<Move> GetMoves(GameState state0)
        {
            var owner = state0.Players.SingleOrDefault(p => state0.Inventory[p].Djinns.Contains(this));
            if (state0.Phase == Phase.End && owner != null)
            {
                if (state0.Inventory[owner].Resources[Resource.Slave] >= 2)
                {
                    foreach (var wild in Enum.GetValues(typeof(Resource)).Cast<Resource>().Except(Resource.Slave))
                    {
                        yield return new TradeSlavesForResourceMove(state0, owner, wild);
                    }
                }
            }
        }

        public class TradeSlavesForResourceMove : Move
        {
            private readonly Resource resource;

            public TradeSlavesForResourceMove(GameState state0, PlayerToken owner, Resource resource)
                : base(state0, owner, s1 =>
                {
                    var inventory = s1.Inventory[owner];
                    return s1.With(
                        inventory: s1.Inventory.SetItem(owner, inventory.With(resources: inventory.Resources.Remove(Resource.Slave, 2).Add(resource))));
                })
            {
                this.resource = resource;
            }

            public Resource Resource
            {
                get { return this.resource; }
            }

            public override string ToString()
            {
                return string.Format("Trade {0},{0} for {1}", Resource.Slave, this.resource);
            }
        }
    }
}
