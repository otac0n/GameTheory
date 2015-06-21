namespace GameTheory.Games.FiveTribes
{
    public class AssasinationTable
    {
        private readonly bool hasProtection;
        private readonly int killCount;

        public AssasinationTable()
            : this(false, 1)
        {
        }

        public AssasinationTable(bool hasProtection, int killCount)
        {
            this.hasProtection = hasProtection;
            this.killCount = killCount;
        }

        public bool HasProtection
        {
            get { return this.hasProtection; }
        }

        public int KillCount
        {
            get { return this.killCount; }
        }

        public AssasinationTable With(bool? hasProtection = null, int? killCount = null)
        {
            return new AssasinationTable(
                hasProtection ?? this.hasProtection,
                killCount ?? this.killCount);
        }
    }
}
