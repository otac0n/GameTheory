namespace GameTheory.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using KnowledgeInterchangeFormat.Expressions;

    internal class DependencyAnalyzer
    {
        public static ImmutableDictionary<(Constant, int), (int, ImmutableHashSet<(Constant, int)>)> Analyze(KnowledgeBase knowledgeBase)
        {
            var walker = new DependencyWalker();
            walker.Walk((Expression)knowledgeBase);
            var dependencies = walker.Result;

            var result = ImmutableDictionary.CreateBuilder<(Constant, int), (int, ImmutableHashSet<(Constant, int)>)>();

            var index = 0;
            var stack = new Stack<(Constant, int)>();
            var ruleData = new Dictionary<(Constant, int), RuleData>();
            RuleData StrongConnect((Constant, int) v)
            {
                var vData = ruleData[v] = new RuleData
                {
                    Index = index,
                    LowLink = index,
                };

                index++;
                stack.Push(v);

                foreach (var w in dependencies[v])
                {
                    if (!ruleData.TryGetValue(w, out var wData))
                    {
                        wData = StrongConnect(w);
                        vData.LowLink = Math.Min(vData.LowLink, wData.LowLink);
                    }
                    else if (stack.Contains(w))
                    {
                        vData.LowLink = Math.Min(vData.LowLink, wData.Index);
                    }
                }

                if (vData.LowLink == vData.Index)
                {
                    (Constant, int) w;
                    do
                    {
                        w = stack.Pop();
                        result.Add(w, (vData.LowLink, dependencies[w]));
                    }
                    while (!object.Equals(w, v));
                }

                return vData;
            }

            foreach (var dep in dependencies)
            {
                if (!ruleData.ContainsKey(dep.Key))
                {
                    StrongConnect(dep.Key);
                }
            }

            return result.ToImmutable();
        }

        private class RuleData
        {
            public int Index { get; set; }

            public int LowLink { get; set; }
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
