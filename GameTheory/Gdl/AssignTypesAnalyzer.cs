namespace GameTheory.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using Intervals;
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
            var movesType = new IntersectionType();
            movesType.Expressions = movesType.Expressions.Add(legal.Arguments[1]);
            does.Arguments[1].ReturnType = movesType;

            var distinct = (RelationInfo)expressionTypes[("DISTINCT", 2)];

            new TypeUsageWalker(expressionTypes, containedVariables).Walk((Expression)knowledgeBase);
            var roleUnion = (UnionType)role.Arguments[0].ReturnType;
            var roleEnum = EnumType.Create("Role", roleUnion.Expressions.Cast<ObjectInfo>());
            does.Arguments[0].ReturnType = legal.Arguments[0].ReturnType = goal.Arguments[0].ReturnType = role.Arguments[0].ReturnType;
            goal.Arguments[1].ReturnType = NumberType.Instance;
            distinct.Arguments[1].ReturnType = distinct.Arguments[0].ReturnType = ObjectType.Instance;
            ReduceTypes(expressionTypes);

            return expressionTypes;
        }

        private static void ReduceTypes(Dictionary<(string, int), ExpressionInfo> expressionTypes)
        {
            var unionTypesCache = new Dictionary<ImmutableHashSet<ExpressionInfo>, UnionType>(
                expressionTypes.Count,
                new ImmutableHashSetEqualityComparer<ExpressionInfo>());

            bool changed;
            do
            {
                changed = false;
                unionTypesCache.Clear();
                var enumCandidates = new List<(RelationInfo, UnionType)>();
                ExpressionTypeVisitor.Visit(
                    expressionTypes.Values,
                    expression =>
                    {
                        switch (expression.ReturnType)
                        {
                            case UnionType unionType:
                                var nones = unionType.Expressions.Where(e => e.ReturnType == NoneType.Instance);
                                if (nones.Any())
                                {
                                    unionType.Expressions = unionType.Expressions.Except(nones);
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
                                    // TODO: Use an Aggregate expression to avoid multiple LINQ loops.
                                    if (unionType.Expressions.All(e => e.ReturnType is NumberRangeType || e.ReturnType == NumberType.Instance))
                                    {
                                        if (unionType.Expressions.Any(e => e.ReturnType == NumberType.Instance && !(e is ObjectInfo)))
                                        {
                                            expression.ReturnType = NumberType.Instance;
                                            changed = true;
                                        }
                                        else
                                        {
                                            var values = unionType.Expressions.Select(e =>
                                            {
                                                return e is ObjectInfo objectInfo
                                                    ? NumberRangeType.GetInstance((int)objectInfo.Value, (int)objectInfo.Value)
                                                    : (NumberRangeType)e.ReturnType;
                                            }).Simplify();

                                            expression.ReturnType = values.Count == 1
                                                ? (ExpressionType)values.Single()
                                                : NumberType.Instance;
                                            changed = true;
                                        }
                                    }
                                    else if (unionType.Expressions.All(e => e.ReturnType is EnumType))
                                    {
                                        var enums = unionType.Expressions.Select(e => (EnumType)e.ReturnType).Distinct().ToList();
                                        if (enums.Count == 1)
                                        {
                                            expression.ReturnType = enums.Single();
                                            changed = true;
                                        }
                                    }
                                    else
                                    {
                                        var nestedUnions = unionType.Expressions.Where(e => e.ReturnType is UnionType).ToList();
                                        if (nestedUnions.Any())
                                        {
                                            unionType.Expressions = unionType.Expressions
                                                .Union(nestedUnions.SelectMany(e => ((UnionType)e.ReturnType).Expressions))
                                                .Except(nestedUnions);
                                            changed = true;
                                        }
                                    }
                                }

                                break;

                            case IntersectionType intersectionType:
                                var objects = intersectionType.Expressions.Where(e => e.ReturnType == ObjectType.Instance);
                                if (objects.Any())
                                {
                                    intersectionType.Expressions = intersectionType.Expressions.Except(objects);
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

                                break;
                        }

                        {
                            if (expression.ReturnType is UnionType unionType)
                            {
                                if (unionTypesCache.TryGetValue(unionType.Expressions, out var found))
                                {
                                    expression.ReturnType = unionType = found;
                                }
                                else
                                {
                                    unionTypesCache[unionType.Expressions] = unionType;
                                }

                                if (expression is ArgumentInfo argument &&
                                    argument.Expression is RelationInfo relation &&
                                    relation.Arity == 1 &&
                                    unionType.Expressions.Count >= 2 &&
                                    unionType.Expressions.All(e => e is ObjectInfo && e.ReturnType is ObjectType))
                                {
                                    enumCandidates.Add((relation, unionType));
                                }
                            }
                        }
                    });

                var count = enumCandidates.Count;
                if (!changed && count > 0)
                {
                    // TODO: Keep an array of trueForAll, and only check each pair once.
                    for (var i = 0; i < count; i++)
                    {
                        (var relation, var unionType) = enumCandidates[i];
                        var trueForAll = true;
                        for (var j = 0; j < count; j++)
                        {
                            if (i == j)
                            {
                                continue;
                            }

                            (_, var otherUnion) = enumCandidates[j];
                            if (unionType.Expressions.Overlaps(otherUnion.Expressions) && !unionType.Expressions.IsProperSubsetOf(otherUnion.Expressions))
                            {
                                trueForAll = false;
                                break;
                            }
                        }

                        if (trueForAll)
                        {
                            relation.Arguments[0].ReturnType = EnumType.Create(relation.Id, unionType.Expressions.Cast<ObjectInfo>());
                            changed = true;
                        }
                    }
                }
            }
            while (changed);
        }

        private enum VariableDirection
        {
            None = 0,
            In,
            Out,
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
                var previousDirection = this.variableDirection;
                try
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
                finally
                {
                    this.variableDirection = previousDirection;
                }
            }

            public override void Walk(KnowledgeBase knowledgeBase)
            {
                this.variableDirection = VariableDirection.Out;

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
                        unionType.Expressions = unionType.Expressions.Add(expressionInfo);
                        break;

                    case IntersectionType intersectionType:
                        intersectionType.Expressions = intersectionType.Expressions.Add(expressionInfo);
                        break;

                    case StructType structType:
                        structType.Objects = structType.Objects.Add(expressionInfo);
                        break;

                    default:
                        if (variableInfo.ReturnType != expressionInfo.ReturnType)
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
                    var valueExpression = this.GetExpressionInfo(arguments[i]);

                    switch (this.variableDirection)
                    {
                        case VariableDirection.In:
                            if (valueExpression is VariableInfo variableInfo)
                            {
                                AddUsage(variableInfo, relationInfo.Arguments[i]);
                            }

                            break;

                        case VariableDirection.Out:
                            AddUsage(relationInfo.Arguments[i], valueExpression);
                            break;
                    }
                }
            }

            private ExpressionInfo GetExpressionInfo(Term arg)
            {
                switch (arg)
                {
                    case Constant constant:
                        return this.expressionTypes[(constant.Id, 0)];

                    case ImplicitFunctionalTerm implicitFunctionalTerm:
                        return this.expressionTypes[(implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count)];

                    case IndividualVariable individualVariable:
                        return this.variableTypes[individualVariable];

                    default:
                        throw new NotImplementedException();
                }
            }
        }
        private class ImmutableHashSetEqualityComparer<T> : IEqualityComparer<ImmutableHashSet<ExpressionInfo>>
        {
            public bool Equals(ImmutableHashSet<ExpressionInfo> x, ImmutableHashSet<ExpressionInfo> y) =>
                object.ReferenceEquals(x, y) || (!(x is null) && !(y is null) && x.SetEquals(y));

            public int GetHashCode(ImmutableHashSet<ExpressionInfo> obj) =>
                obj is null ? 0 : obj.Count;
        }
    }
}
