// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using KnowledgeInterchangeFormat.Expressions;

    public static class ContainedVariablesAnalyzer
    {
        public static Dictionary<Expression, ImmutableHashSet<Variable>> Analyze(KnowledgeBase knowledgeBase)
        {
            var results = new Dictionary<Expression, ImmutableHashSet<Variable>>();
            new ContainedVariablesWalker(results).Walk((Expression)knowledgeBase);
            return results;
        }

        private class ContainedVariablesWalker : SupportedExpressionsTreeWalker
        {
            private readonly Dictionary<Expression, ImmutableHashSet<Variable>> allContainedVariables;
            private ImmutableHashSet<Variable> containedVariables = ImmutableHashSet<Variable>.Empty;

            public ContainedVariablesWalker(Dictionary<Expression, ImmutableHashSet<Variable>> constantTypes)
            {
                this.allContainedVariables = constantTypes;
            }

            public override void Walk(Expression expression)
            {
                var originalVariables = this.containedVariables;

                this.containedVariables = ImmutableHashSet<Variable>.Empty;
                base.Walk(expression);
                this.allContainedVariables[expression] = this.containedVariables;

                this.containedVariables = originalVariables.Union(this.containedVariables);
            }

            public override void Walk(Variable variable)
            {
                this.containedVariables = this.containedVariables.Add(variable);
            }
        }
    }
}
