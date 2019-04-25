namespace GameTheory.Gdl
{
    using KnowledgeInterchangeFormat.Expressions;

    public static class KnownConstants
    {
        public static readonly (Constant constant, int arity) Base = (new Constant("BASE", "base"), 1);
        public static readonly (Constant constant, int arity) Role = (new Constant("ROLE", "role"), 1);
        public static readonly (Constant constant, int arity) Init = (new Constant("INIT", "init"), 1);
        public static readonly (Constant constant, int arity) True = (new Constant("TRUE", "true"), 1);
        public static readonly (Constant constant, int arity) Input = (new Constant("INPUT", "input"), 2);
        public static readonly (Constant constant, int arity) Does = (new Constant("DOES", "does"), 2);
        public static readonly (Constant constant, int arity) Next = (new Constant("NEXT", "next"), 1);
        public static readonly (Constant constant, int arity) Legal = (new Constant("LEGAL", "legal"), 2);
        public static readonly (Constant constant, int arity) Goal = (new Constant("GOAL", "goal"), 2);
        public static readonly (Constant constant, int arity) Terminal = (new Constant("TERMINAL", "terminal"), 0);
        public static readonly (Constant constant, int arity) Distinct = (new Constant("DISTINCT", "distinct"), 2);
        public static readonly (Constant constant, int arity) Noop = (new Constant("NOOP", "noop"), 0);
    }
}
