// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Gdl.Types;
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
                        expressionTypes[kvp.Key] = new LogicalInfo(kvp.Key.Item1);
                        break;

                    case ConstantType.Object:
                        var id = kvp.Key.Item1;
                        var objectInfo = new ObjectInfo(id);
                        expressionTypes[kvp.Key] = objectInfo;

                        if (int.TryParse(id, out var value) && value.ToString().Equals(id, StringComparison.OrdinalIgnoreCase))
                        {
                            objectInfo.ReturnType = NumberType.Instance;
                            objectInfo.Value = value;
                        }

                        break;

                    case ConstantType.Relation:
                        expressionTypes[kvp.Key] = new RelationInfo(kvp.Key.Item1, kvp.Key.Item2);
                        break;
                }
            }

            var init = (RelationInfo)expressionTypes[("INIT", 1)];
            var @true = (RelationInfo)expressionTypes[("TRUE", 1)];
            var next = (RelationInfo)expressionTypes[("NEXT", 1)];
            init.Arguments[0].ReturnType = @true.Arguments[0].ReturnType = next.Arguments[0].ReturnType = new StructType("State");

            var role = (RelationInfo)expressionTypes[("ROLE", 1)];
            var does = (RelationInfo)expressionTypes[("DOES", 2)];
            var legal = (RelationInfo)expressionTypes[("LEGAL", 2)];
            var goal = (RelationInfo)expressionTypes[("GOAL", 2)];
            role.Arguments[0].ReturnType = does.Arguments[0].ReturnType = legal.Arguments[0].ReturnType = goal.Arguments[0].ReturnType = new UnionType();
            does.Arguments[1].ReturnType = legal.Arguments[1].ReturnType = new UnionType();
            goal.Arguments[1].ReturnType = NumberType.Instance;

            var distinct = (RelationInfo)expressionTypes[("DISTINCT", 2)];
            distinct.Arguments[1].ReturnType = distinct.Arguments[0].ReturnType = ObjectType.Instance;

            new TypeUsageWalker(result, expressionTypes).Walk((Expression)result.KnowledgeBase);
        }

        [Flags]
        private enum VariableDirection
        {
            None = 0,
            In = 0x01,
            Out = 0x02,
            Both = 0x03,
        }

        private class TypeUsageWalker : SupportedExpressionsTreeWalker
        {
            private readonly CompileResult result;
            private readonly Dictionary<(string, int), ExpressionInfo> expressionTypes;
            private Dictionary<IndividualVariable, VariableInfo> variableTypes;
            private VariableDirection variableDirection;

            public TypeUsageWalker(CompileResult result, Dictionary<(string, int), ExpressionInfo> expressionTypes)
            {
                this.result = result;
                this.expressionTypes = expressionTypes;
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                var relationInfo = (RelationInfo)this.expressionTypes[(implicitRelationalSentence.Relation.Id, implicitRelationalSentence.Arguments.Count)];
                this.AddArgumentUsages(implicitRelationalSentence.Arguments, relationInfo);
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                var functionInfo = (FunctionInfo)this.expressionTypes[(implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count)];
                this.AddArgumentUsages(implicitFunctionalTerm.Arguments, functionInfo);
                base.Walk(implicitFunctionalTerm);
            }

            public override void Walk(Implication implication)
            {
                this.variableDirection = VariableDirection.In;
                if (implication.Antecedents != null)
                {
                    foreach (var sentence in implication.Antecedents)
                    {
                        this.Walk((Expression)sentence);
                    }
                }

                this.variableDirection = VariableDirection.Out;
                if (implication.Consequent != null)
                {
                    this.Walk((Expression)implication.Consequent);
                }
            }

            public override void Walk(KnowledgeBase knowledgeBase)
            {
                this.variableDirection = VariableDirection.Both;

                foreach (var form in knowledgeBase.Forms)
                {
                    this.variableTypes = this.result.ContainedVariables[form].ToDictionary(r => (IndividualVariable)r, r => new VariableInfo(r.Id));
                    this.Walk((Expression)form);
                    this.variableTypes = null;
                }

                this.variableDirection = VariableDirection.None;
            }

            private static void AddUsage(ArgumentInfo argumentInfo, ExpressionInfo expressionInfo)
            {
                switch (argumentInfo.ReturnType)
                {
                    case null:
                        argumentInfo.ReturnType = new UnionType()
                        {
                            expressionInfo,
                        };
                        break;

                    case UnionType unionType:
                        unionType.Add(expressionInfo);
                        break;

                    case StructType structType:
                        structType.Add(expressionInfo);
                        break;

                    default:
                        if (argumentInfo.ReturnType != expressionInfo.ReturnType)
                        {
                            throw new InvalidOperationException();
                        }

                        break;
                }
            }

            private void AddArgumentUsages(ImmutableList<Term> arguments, ExpressionWithArgumentsInfo relationInfo)
            {
                var arity = arguments.Count;
                for (var i = 0; i < arity; i++)
                {
                    switch (arguments[i])
                    {
                        case Constant constant:
                            AddUsage(relationInfo.Arguments[i], this.expressionTypes[(constant.Id, 0)]);
                            break;
                        case ImplicitFunctionalTerm implicitFunctionalTerm:
                            AddUsage(relationInfo.Arguments[i], this.expressionTypes[(implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count)]);
                            break;
                        case IndividualVariable individualVariable:
                            var variableInfo = this.variableTypes[individualVariable];

                            switch (this.variableDirection)
                            {
                                case VariableDirection.In:
                                    break;

                                case VariableDirection.Out:
                                    AddUsage(relationInfo.Arguments[i], variableInfo);
                                    break;

                                case VariableDirection.Both:
                                    break;
                            }

                            break;
                    }
                }
            }
        }
    }
}
