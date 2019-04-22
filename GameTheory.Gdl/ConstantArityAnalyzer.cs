// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using KnowledgeInterchangeFormat.Expressions;

    public static class ConstantArityAnalyzer
    {
        public static ImmutableDictionary<(string, int), ConstantType> Analyze(KnowledgeBase knowledgeBase)
        {
            var results = new Dictionary<(string, int), ConstantType>
            {
                [("ROLE", 1)] = ConstantType.Relation,
                [("INIT", 1)] = ConstantType.Relation,
                [("TRUE", 1)] = ConstantType.Relation,
                [("DOES", 2)] = ConstantType.Relation,
                [("NEXT", 1)] = ConstantType.Relation,
                [("LEGAL", 2)] = ConstantType.Relation,
                [("GOAL", 2)] = ConstantType.Relation,
                [("TERMINAL", 0)] = ConstantType.Logical,
                [("DISTINCT", 2)] = ConstantType.Relation,
            };

            for (var i = 0; i <= 100; i++)
            {
                results[(i.ToString(), 0)] = ConstantType.Object;
            }

            new ConstantArityWalker(results).Walk((Expression)knowledgeBase);

            foreach (var item in results.ToList())
            {
                if (item.Value == ConstantType.Unknown)
                {
                    results[item.Key] = ConstantType.Object;
                }
            }

            return results.ToImmutableDictionary();
        }

        private class ConstantArityWalker : SupportedExpressionsTreeWalker
        {
            private readonly Dictionary<(string, int), ConstantType> constantTypes;

            public ConstantArityWalker(Dictionary<(string, int), ConstantType> constantTypes)
            {
                this.constantTypes = constantTypes;
            }

            public override void Walk(Constant constant)
            {
                this.AddResult((constant.Id, 0), ConstantType.Object);
            }

            public override void Walk(ConstantSentence constantSentence)
            {
                this.AddResult((constantSentence.Constant.Id, 0), ConstantType.Logical);
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                this.AddResult((implicitRelationalSentence.Relation.Id, implicitRelationalSentence.Arguments.Count), ConstantType.Relation);
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                this.AddResult((implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count), ConstantType.Function);
                base.Walk(implicitFunctionalTerm);
            }

            private void AddResult((string id, int arity) key, ConstantType type)
            {
                if (!this.constantTypes.TryGetValue(key, out var value) || value == ConstantType.Unknown)
                {
                    this.constantTypes[key] = type;
                }
                else if (type != ConstantType.Unknown && value != type && value != ConstantType.Invalid)
                {
                    this.constantTypes[key] = ConstantType.Invalid;
                }
            }
        }
    }
}
