namespace GameTheory.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat.Expressions;

    public static class AssignTypesAnalyzer
    {
        public static Dictionary<(string, int), ExpressionInfo> Analyze(KnowledgeBase knowledgeBase, Dictionary<(string, int), ConstantType> constantTypes, Dictionary<Expression, HashSet<Variable>> containedVariables)
        {
            var expressionTypes = new Dictionary<(string, int), ExpressionInfo>();

            foreach (var kvp in constantTypes)
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
                        ObjectInfo objectInfo;
                        if (int.TryParse(id, out var value) && value.ToString().Equals(id, StringComparison.OrdinalIgnoreCase))
                        {
                            objectInfo = new ObjectInfo(id, NumberType.Instance, value);
                        }
                        else
                        {
                            objectInfo = new ObjectInfo(id, new ObjectType(id), id);
                        }

                        expressionTypes[kvp.Key] = objectInfo;
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

            new TypeUsageWalker(expressionTypes, containedVariables).Walk((Expression)knowledgeBase);

            bool changed;
            do
            {
                changed = false;
                ExpressionTypeVisitor.Visit(
                    expressionTypes.Values,
                    expression =>
                    {
                        switch (expression.ReturnType)
                        {
                            case UnionType unionType:
                                if (unionType.Expressions.RemoveWhere(e => e.ReturnType == NoneType.Instance) > 0)
                                {
                                    changed = true;
                                }

                                if (unionType.Expressions.Count == 0)
                                {
                                    expression.ReturnType = ObjectType.Instance;
                                    changed = true;
                                }
                                else if (unionType.Expressions.Count == 1)
                                {
                                    expression.ReturnType = unionType.Expressions.Single().ReturnType;
                                    changed = true;
                                }
                                else
                                {
                                    var nestedUnions = unionType.Expressions.Where(e => e.ReturnType is UnionType).ToList();
                                    if (nestedUnions.Any())
                                    {
                                        unionType.Expressions.UnionWith(nestedUnions.SelectMany(e => ((UnionType)e.ReturnType).Expressions));
                                        unionType.Expressions.ExceptWith(nestedUnions);
                                        changed = true;
                                    }
                                }

                                break;

                            case IntersectionType intersectionType:
                                if (intersectionType.Expressions.RemoveWhere(e => e.ReturnType == ObjectType.Instance) > 0)
                                {
                                    changed = true;
                                }

                                if (intersectionType.Expressions.Count == 0)
                                {
                                    expression.ReturnType = NoneType.Instance;
                                    changed = true;
                                }
                                else if (intersectionType.Expressions.Count == 1)
                                {
                                    expression.ReturnType = intersectionType.Expressions.Single().ReturnType;
                                    changed = true;
                                }
                                else
                                {
                                }

                                break;
                        }
                    },
                    type =>
                    {
                    });
            }
            while (changed);

            return expressionTypes;
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
            private readonly Dictionary<(string, int), ExpressionInfo> expressionTypes;
            private readonly Dictionary<Expression, HashSet<Variable>> containedVariables;
            private Dictionary<IndividualVariable, VariableInfo> variableTypes;
            private VariableDirection variableDirection;

            public TypeUsageWalker(Dictionary<(string, int), ExpressionInfo> expressionTypes, Dictionary<Expression, HashSet<Variable>> containedVariables)
            {
                this.expressionTypes = expressionTypes;
                this.containedVariables = containedVariables;
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
                    this.variableTypes = this.containedVariables[form].ToDictionary(r => (IndividualVariable)r, r => new VariableInfo(r.Id));
                    this.Walk((Expression)form);
                    this.variableTypes = null;
                }

                this.variableDirection = VariableDirection.None;
            }

            private static void AddUsage(VariableInfo variableInfo, ExpressionInfo expressionInfo)
            {
                switch (variableInfo.ReturnType)
                {
                    case UnionType unionType:
                        unionType.Expressions.Add(expressionInfo);
                        break;

                    case StructType structType:
                        structType.Objects.Add(expressionInfo);
                        break;

                    default:
                        if (variableInfo.ReturnType != expressionInfo.ReturnType)
                        {
                            throw new InvalidOperationException();
                        }

                        break;
                }
            }

            private static void MakeVariableAssignableToArgument(VariableInfo variableInfo, ArgumentInfo argumentInfo)
            {
                if (variableInfo.ReturnType is IntersectionType intersectionType)
                {
                    intersectionType.Expressions.Add(argumentInfo);
                }
                else
                {
                    throw new InvalidOperationException();
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
                                    MakeVariableAssignableToArgument(variableInfo, relationInfo.Arguments[i]);
                                    break;

                                case VariableDirection.Out:
                                    AddUsage(relationInfo.Arguments[i], variableInfo);
                                    break;

                                case VariableDirection.Both:
                                    AddUsage(variableInfo, relationInfo.Arguments[i]);
                                    AddUsage(relationInfo.Arguments[i], variableInfo);
                                    break;
                            }

                            break;
                    }
                }
            }
        }
    }
}
