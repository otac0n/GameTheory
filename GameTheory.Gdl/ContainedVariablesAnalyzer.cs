// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using KnowledgeInterchangeFormat.Expressions;

    public static class ContainedVariablesAnalyzer
    {
        public static ImmutableDictionary<Expression, ImmutableHashSet<IndividualVariable>> Analyze(KnowledgeBase knowledgeBase)
        {
            var walker = new ContainedVariablesWalker();
            walker.Walk((Expression)knowledgeBase);
            return walker.AllContainedVariables.ToImmutableDictionary();
        }

        private class ContainedVariablesWalker : SupportedExpressionsTreeWalker
        {
            private ImmutableHashSet<IndividualVariable> containedVariables = ImmutableHashSet<IndividualVariable>.Empty;

            public Dictionary<Expression, ImmutableHashSet<IndividualVariable>> AllContainedVariables { get; } = new Dictionary<Expression, ImmutableHashSet<IndividualVariable>>();

            public override void Walk(Expression expression)
            {
                var originalVariables = this.containedVariables;
                this.containedVariables = ImmutableHashSet<IndividualVariable>.Empty;

                base.Walk(expression);

                this.AllContainedVariables[expression] = this.containedVariables;
                this.containedVariables = originalVariables.Union(this.containedVariables);
            }

            public override void Walk(IndividualVariable individualVariable)
            {
                this.containedVariables = this.containedVariables.Add(individualVariable);
            }
        }
    }
}
