// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System.Collections.Generic;
    using KnowledgeInterchangeFormat.Expressions;

    public static class ContainedVariablesAnalyzer
    {
        public static Dictionary<Expression, HashSet<Variable>> Analyze(KnowledgeBase knowledgeBase)
        {
            var results = new Dictionary<Expression, HashSet<Variable>>();
            new ContainedVariablesWalker(results).Walk((Expression)knowledgeBase);
            return results;
        }

        private class ContainedVariablesWalker : SupportedExpressionsTreeWalker
        {
            private readonly Dictionary<Expression, HashSet<Variable>> allContainedVariables;
            private HashSet<Variable> containedVariables = new HashSet<Variable>();

            public ContainedVariablesWalker(Dictionary<Expression, HashSet<Variable>> constantTypes)
            {
                this.allContainedVariables = constantTypes;
            }

            public override void Walk(Expression expression)
            {
                var originalVariables = this.containedVariables;

                base.Walk(expression);

                this.allContainedVariables[expression] = this.containedVariables;
                originalVariables.UnionWith(this.containedVariables);
                this.containedVariables = originalVariables;
            }

            public override void Walk(Variable variable)
            {
                this.containedVariables.Add(variable);
            }
        }
    }
}
