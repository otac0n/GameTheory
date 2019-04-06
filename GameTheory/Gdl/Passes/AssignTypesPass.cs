// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat;
    using KnowledgeInterchangeFormat.Expressions;

    internal class AssignTypesPass : CompilePass
    {
        public override IList<string> BlockedByErrors => new[]
        {
            EnforceGdlRestrictions.RoleRelationUsedInRuleError,
            EnforceGdlRestrictions.InitRelationUsedInRuleBodyError,
            EnforceGdlRestrictions.InitRelationDependencyError,
            EnforceGdlRestrictions.TrueRelationUsedOutsideRuleBodyError,
            EnforceGdlRestrictions.NextRelationUsedOutsideRuleHeadError,
            EnforceGdlRestrictions.DoesUsedOutsideRuleBodyError,
            EnforceGdlRestrictions.DoesRelationDependencyError,
        };

        public override IList<string> ErrorsProduced => Array.Empty<string>();

        public override void Run(CompileResult result)
        {
            var expressionTypes = new Dictionary<(string, int), ExpressionInfo>();

            foreach (var kvp in result.ConstantTypes)
            {
                switch (kvp.Value)
                {
                    case ConstantType.Function:
                        expressionTypes[kvp.Key] = new FunctionInfo(kvp.Key.Item1, kvp.Key.Item2);
                        break;
                    case ConstantType.Logical:
                        expressionTypes[kvp.Key] = new LogicalInfo(kvp.Key.Item1, kvp.Key.Item2);
                        break;
                    case ConstantType.Object:
                        expressionTypes[kvp.Key] = new ObjectInfo(kvp.Key.Item1, kvp.Key.Item2);
                        break;
                    case ConstantType.Relation:
                        expressionTypes[kvp.Key] = new RelationInfo(kvp.Key.Item1, kvp.Key.Item2);
                        break;
                }
            }

            new TypesWalker(result, expressionTypes).Walk((Expression)result.KnowledgeBase);
        }

        private class TypesWalker : SupportedExpressionsTreeWalker
        {
            private readonly CompileResult result;
            private readonly Dictionary<(string, int), ExpressionInfo> expressionTypes;

            public TypesWalker(CompileResult result, Dictionary<(string, int), ExpressionInfo> expressionTypes)
            {
                this.result = result;
                this.expressionTypes = expressionTypes;
            }

            public override void Walk(Constant constant)
            {
                var objectInfo = (ObjectInfo)this.expressionTypes[(constant.Id, 0)];
            }

            public override void Walk(ConstantSentence constantSentence)
            {
                var logicalInfo = (LogicalInfo)this.expressionTypes[(constantSentence.Constant.Id, 0)];
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                var relationInfo = (RelationInfo)this.expressionTypes[(implicitRelationalSentence.Relation.Id, implicitRelationalSentence.Arguments.Count)];
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                var logicalInfo = (FunctionInfo)this.expressionTypes[(implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count)];
                base.Walk(implicitFunctionalTerm);
            }
        }
    }
}
