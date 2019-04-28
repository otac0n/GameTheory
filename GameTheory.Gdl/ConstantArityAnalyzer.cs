// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using KnowledgeInterchangeFormat.Expressions;

    public static class ConstantArityAnalyzer
    {
        public static ImmutableDictionary<(Constant, int), ConstantType> Analyze(KnowledgeBase knowledgeBase)
        {
            var results = new Dictionary<(Constant, int), ConstantType>
            {
                [KnownConstants.Role] = ConstantType.Relation,
                [KnownConstants.Init] = ConstantType.Relation,
                [KnownConstants.True] = ConstantType.Relation,
                [KnownConstants.Does] = ConstantType.Relation,
                [KnownConstants.Next] = ConstantType.Relation,
                [KnownConstants.Legal] = ConstantType.Relation,
                [KnownConstants.Goal] = ConstantType.Relation,
                [KnownConstants.Terminal] = ConstantType.Logical,
                [KnownConstants.Distinct] = ConstantType.Relation,
            };

            for (var i = 0; i <= 100; i++)
            {
                results[(new Constant(i.ToString()), 0)] = ConstantType.Object;
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
            private readonly Dictionary<(Constant, int), ConstantType> constantTypes;

            public ConstantArityWalker(Dictionary<(Constant, int), ConstantType> constantTypes)
            {
                this.constantTypes = constantTypes;
            }

            public override void Walk(Constant constant)
            {
                this.AddResult((constant, 0), ConstantType.Object);
            }

            public override void Walk(ConstantSentence constantSentence)
            {
                this.AddResult((constantSentence.Constant, 0), ConstantType.Logical);
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                this.AddResult((implicitRelationalSentence.Relation, implicitRelationalSentence.Arguments.Count), ConstantType.Relation);
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                this.AddResult((implicitFunctionalTerm.Function, implicitFunctionalTerm.Arguments.Count), ConstantType.Function);
                base.Walk(implicitFunctionalTerm);
            }

            private void AddResult((Constant id, int arity) key, ConstantType type)
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
