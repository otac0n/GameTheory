namespace GameTheory.Games.FiveTribes
{
    using System.Collections.Immutable;

    public class Inventory
    {
        private readonly ImmutableList<Djinn> djinns;
        private readonly int goldCoins;
        private readonly EnumCollection<Meeple> meeples;
        private readonly EnumCollection<Resource> resources;

        public Inventory()
        {
            this.djinns = ImmutableList<Djinn>.Empty;
            this.meeples = EnumCollection<Meeple>.Empty;
            this.goldCoins = 50;
            this.resources = EnumCollection<Resource>.Empty;
        }

        private Inventory(ImmutableList<Djinn> djinns, EnumCollection<Meeple> meeples, int goldCoins, EnumCollection<Resource> resources)
        {
            this.djinns = djinns;
            this.meeples = meeples;
            this.goldCoins = goldCoins;
            this.resources = resources;
        }

        public ImmutableList<Djinn> Djinns
        {
            get { return this.djinns; }
        }

        public int GoldCoins
        {
            get { return this.goldCoins; }
        }

        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        public EnumCollection<Resource> Resources
        {
            get { return this.resources; }
        }

        public Inventory With(ImmutableList<Djinn> djinns = null, int? goldCoins = null, EnumCollection<Meeple> meeples = null, EnumCollection<Resource> resources = null)
        {
            return new Inventory(
                djinns ?? this.djinns,
                meeples ?? this.meeples,
                goldCoins ?? this.goldCoins,
                resources ?? this.resources);
        }
    }
}
