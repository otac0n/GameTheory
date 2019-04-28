namespace GameTheory.Gdl
{
    using System.Collections.Immutable;
    using KnowledgeInterchangeFormat.Expressions;

    internal class DependencyAnalyzer
    {
        public static ImmutableDictionary<(Constant, int), ImmutableHashSet<(Constant, int)>> Analyze(KnowledgeBase knowledgeBase)
        {
            var walker = new DependencyWalker();
            walker.Walk((Expression)knowledgeBase);
            return walker.Result;
        }

        private class DependencyWalker : SupportedExpressionsTreeWalker
        {
            private (Constant constant, int arity) implicated;

            public ImmutableDictionary<(Constant, int), ImmutableHashSet<(Constant, int)>> Result { get; private set; } = ImmutableDictionary<(Constant, int), ImmutableHashSet<(Constant, int)>>.Empty;

            public override void Walk(KnowledgeBase knowledgeBase)
            {
                foreach (var form in knowledgeBase.Forms)
                {
                    this.implicated = ((Sentence)form).GetImplicatedConstantWithArity();
                    this.EnsureNode(this.implicated);

                    if (form is Implication)
                    {
                        this.Walk((Expression)form);
                    }
                }
            }

            public override void Walk(Implication implication)
            {
                // Skip the consequent.
                foreach (var sentence in implication.Antecedents)
                {
                    this.Walk((Expression)sentence);
                }
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                var key = (implicitRelationalSentence.Relation, implicitRelationalSentence.Arguments.Count);
                this.EnsureNode(key);
                this.Result = this.Result.SetItem(
                    key,
                    this.Result[key].Add(this.implicated));
            }

            private void EnsureNode((Constant constant, int arity) key)
            {
                if (!this.Result.ContainsKey(key))
                {
                    this.Result = this.Result.SetItem(key, ImmutableHashSet<(Constant, int)>.Empty);
                }
            }
        }
    }
}
