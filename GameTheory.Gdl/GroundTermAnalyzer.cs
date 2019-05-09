namespace GameTheory.Gdl
{
    using System.Collections.Immutable;
    using KnowledgeInterchangeFormat.Expressions;

    public static class GroundTermAnalyzer
    {
        internal static ImmutableHashSet<Term> Analyze(KnowledgeBase knowledgeBase, ImmutableDictionary<Expression, ImmutableHashSet<IndividualVariable>> containedVariables)
        {
            var walker = new GroundTermWalker(containedVariables);
            walker.Walk((Expression)knowledgeBase);
            return walker.GroundTerms.ToImmutable();
        }

        private class GroundTermWalker : SupportedExpressionsTreeWalker
        {
            private ImmutableDictionary<Expression, ImmutableHashSet<IndividualVariable>> containedVariables;

            public GroundTermWalker(ImmutableDictionary<Expression, ImmutableHashSet<IndividualVariable>> containedVariables)
            {
                this.containedVariables = containedVariables;
                this.GroundTerms = ImmutableHashSet.CreateBuilder<Term>();
            }

            public ImmutableHashSet<Term>.Builder GroundTerms { get; }

            public override void Walk(ConstantSentence constantSentence)
            {
                // Don't walk further, to avoid interpreting logical relations as terms.
            }

            public override void Walk(Term term)
            {
                if (this.containedVariables[term].IsEmpty)
                {
                    this.GroundTerms.Add(term);
                }

                base.Walk(term);
            }
        }
    }
}
