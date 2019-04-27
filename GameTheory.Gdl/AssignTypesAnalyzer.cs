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
        public static AssignedTypes Analyze(KnowledgeBase knowledgeBase, ImmutableDictionary<(Constant, int), ConstantType> constantTypes, ImmutableDictionary<Expression, ImmutableHashSet<IndividualVariable>> containedVariables)
        {
            var bodies = knowledgeBase.Forms.Cast<Sentence>().ToLookup(f => f.GetImplicatedConstantWithArity());

            var expressionTypes = constantTypes.ToDictionary(kvp => kvp.Key, kvp =>
            {
                switch (kvp.Value)
                {
                    case ConstantType.Function:
                        return (ConstantInfo)new FunctionInfo(kvp.Key.Item1, kvp.Key.Item2);

                    case ConstantType.Logical:
                        return (ConstantInfo)new LogicalInfo(kvp.Key.Item1, bodies[kvp.Key]);

                    case ConstantType.Object:
                        return (ConstantInfo)new ObjectInfo(kvp.Key.Item1);

                    case ConstantType.Relation:
                        return (ConstantInfo)new RelationInfo(kvp.Key.Item1, kvp.Key.Item2, bodies[kvp.Key]);
                }

                throw new InvalidOperationException();
            }).ToImmutableDictionary();
            var variableTypes = knowledgeBase.Forms.Cast<Sentence>().ToImmutableDictionary(s => s, s => containedVariables[s].ToImmutableDictionary(p => p, p => new VariableInfo(p.Id))); // TODO: Carry variable around, so that casing is perserved.
            var assignedTypes = new AssignedTypes(expressionTypes, variableTypes);

            var init = (RelationInfo)expressionTypes[KnownConstants.Init];
            var @true = (RelationInfo)expressionTypes[KnownConstants.True];
            var next = (RelationInfo)expressionTypes[KnownConstants.Next];
            var @base = expressionTypes.ContainsKey(KnownConstants.Base) ? (RelationInfo)expressionTypes[KnownConstants.Base] : null;
            init.Arguments[0].ReturnType = @true.Arguments[0].ReturnType = next.Arguments[0].ReturnType = new StateType();
            if (@base != null)
            {
                @base.Arguments[0].ReturnType = @true.Arguments[0].ReturnType;
            }

            var role = (RelationInfo)expressionTypes[KnownConstants.Role];
            var does = (RelationInfo)expressionTypes[KnownConstants.Does];
            var legal = (RelationInfo)expressionTypes[KnownConstants.Legal];
            var input = expressionTypes.ContainsKey(KnownConstants.Input) ? (RelationInfo)expressionTypes[KnownConstants.Input] : null;
            var goal = (RelationInfo)expressionTypes[KnownConstants.Goal];
            var movesType = new IntersectionType();
            movesType.Expressions = movesType.Expressions.Add(legal.Arguments[1]);
            does.Arguments[1].ReturnType = movesType;
            if (input != null)
            {
                movesType.Expressions = movesType.Expressions.Add(input.Arguments[1]);
            }

            var distinct = (RelationInfo)expressionTypes[KnownConstants.Distinct];

            new TypeUsageWalker(assignedTypes).Walk((Expression)knowledgeBase);
            var roleUnion = (UnionType)role.Arguments[0].ReturnType;
            var roleEnum = EnumType.Create(role, roleUnion.Expressions.Cast<ObjectInfo>());
            does.Arguments[0].ReturnType = legal.Arguments[0].ReturnType = goal.Arguments[0].ReturnType = role.Arguments[0].ReturnType;
            if (input != null)
            {
                input.Arguments[0].ReturnType = role.Arguments[0].ReturnType;
            }

            goal.Arguments[1].ReturnType = NumberRangeType.GetInstance(0, 100);
            distinct.Arguments[1].ReturnType = distinct.Arguments[0].ReturnType = AnyType.Instance;

            ReduceTypes(assignedTypes);
            return assignedTypes;
        }

        private static void ReduceTypes(AssignedTypes assignedTypes)
        {
            var unionTypesCache = new Dictionary<ImmutableHashSet<ExpressionInfo>, UnionType>(
                assignedTypes.ExpressionTypes.Count,
                new ImmutableHashSetEqualityComparer<ExpressionInfo>());
            var intersectionTypesCache = new Dictionary<ImmutableHashSet<ExpressionInfo>, IntersectionType>(
                assignedTypes.ExpressionTypes.Count,
                new ImmutableHashSetEqualityComparer<ExpressionInfo>());

            ImmutableHashSet<ExpressionInfo> FlattenUnion(IEnumerable<ExpressionInfo> expressions)
            {
                var queue = new Queue<ExpressionInfo>(expressions);
                var seen = new HashSet<ExpressionInfo>();
                var result = ImmutableHashSet.CreateBuilder<ExpressionInfo>();
                while (queue.Count > 0)
                {
                    var expr = queue.Dequeue();
                    if (!seen.Add(expr))
                    {
                        continue;
                    }

                    switch (expr.ReturnType)
                    {
                        case ObjectType objectType:
                            result.Add(assignedTypes.ExpressionTypes[(objectType.Constant, 0)]);
                            break;

                        case UnionType unionType:
                            foreach (var inner in unionType.Expressions)
                            {
                                queue.Enqueue(inner);
                            }

                            break;

                        case EnumType enumType:
                            foreach (var inner in enumType.Objects)
                            {
                                result.Add(inner);
                            }

                            break;

                        case NumberRangeType numberRangeType:
                            foreach (var inner in Enumerable.Range(numberRangeType.Start, numberRangeType.End - numberRangeType.Start + 1).Select(i => assignedTypes.ExpressionTypes[(new Constant(i.ToString()), 0)]))
                            {
                                result.Add(inner);
                            }

                            break;

                        default:
                            result.Add(expr);
                            break;
                    }
                }

                return result.ToImmutable();
            }

            bool changed;
            do
            {
                changed = false;
                unionTypesCache.Clear();
                intersectionTypesCache.Clear();
                var enumCandidates = new List<(RelationInfo relation, UnionType unionType)>();
                ExpressionTypeVisitor.Visit(
                    assignedTypes,
                    expression =>
                    {
                        switch (expression.ReturnType)
                        {
                            case UnionType unionType:
                                {
                                    // (X ∪ any) ⇔ any
                                    var anys = unionType.Expressions.Where(e => e.ReturnType is AnyType);
                                    if (anys.Any())
                                    {
                                        expression.ReturnType = AnyType.Instance;
                                        changed = true;
                                    }

                                    // (X ∪ none) ⇔ X
                                    var nones = unionType.Expressions.Where(e => e.ReturnType == NoneType.Instance);
                                    if (nones.Any())
                                    {
                                        var withoutNones = unionType.Expressions.Except(nones);
                                        if (withoutNones.Count > 0)
                                        {
                                            unionType.Expressions = withoutNones;
                                            changed = true;
                                        }
                                    }

                                    if (unionType.Expressions.Contains(expression))
                                    {
                                        unionType.Expressions = unionType.Expressions.Remove(expression);
                                        changed = true;
                                    }

                                    // (X ∪ Y ∪ (X ∩ ...)) ⇔ (X ∪ Y)
                                    var degenerateIntersections = unionType.Expressions.Where(e =>
                                        e.ReturnType is IntersectionType subType &&
                                        (subType.Expressions.Contains(expression) || subType.Expressions.Overlaps(unionType.Expressions)));
                                    if (degenerateIntersections.Any())
                                    {
                                        var withoutDegenerateIntersections = unionType.Expressions.Except(degenerateIntersections);
                                        if (withoutDegenerateIntersections.Count > 0)
                                        {
                                            unionType.Expressions = withoutDegenerateIntersections;
                                            changed = true;
                                        }
                                    }

                                    if (unionType.Expressions.Count == 0)
                                    {
                                        expression.ReturnType = NoneType.Instance;
                                        changed = true;
                                    }
                                    else if (unionType.Expressions.Count == 1)
                                    {
                                        expression.ReturnType = unionType.Expressions.Single().ReturnType;
                                        changed = true;
                                    }
                                    else if (unionType.Expressions.All(e => e.ReturnType is NumberRangeType))
                                    {
                                        var resultingTypes = unionType.Expressions
                                            .Select(e => (NumberRangeType)e.ReturnType)
                                            .Simplify();
                                        if (resultingTypes.Count == 1)
                                        {
                                            expression.ReturnType = (ExpressionType)resultingTypes.Single();
                                            changed = true;
                                        }
                                    }
                                    else if (unionType.Expressions.Select(e => e.ReturnType).Distinct().Take(2).Count() == 1 &&
                                        !(unionType.Expressions.First().ReturnType is EnumType enumType && unionType.Expressions.All(e => e is ObjectInfo) && !unionType.Expressions.SetEquals(enumType.Objects)))
                                    {
                                        expression.ReturnType = unionType.Expressions.First().ReturnType;
                                        changed = true;
                                    }
                                    else
                                    {
                                        // ((X) ∪ (Y ∪ Z)) ⇔ (X ∪ Y ∪ Z)
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
                                {
                                    // (X ∩ none) ⇔ none
                                    var nones = intersectionType.Expressions.Where(e => e.ReturnType == NoneType.Instance);
                                    if (nones.Any())
                                    {
                                        expression.ReturnType = NoneType.Instance;
                                        changed = true;
                                    }

                                    // (X ∩ any) ⇔ X
                                    var anys = intersectionType.Expressions.Where(e => e.ReturnType is AnyType);
                                    if (anys.Any())
                                    {
                                        var withoutAnys = intersectionType.Expressions.Except(anys);
                                        if (withoutAnys.Count > 0)
                                        {
                                            intersectionType.Expressions = withoutAnys;
                                            changed = true;
                                        }
                                    }

                                    if (intersectionType.Expressions.Contains(expression))
                                    {
                                        intersectionType.Expressions = intersectionType.Expressions.Remove(expression);
                                        changed = true;
                                    }

                                    if (intersectionType.Expressions.Count == 0)
                                    {
                                        expression.ReturnType = AnyType.Instance;
                                        changed = true;
                                    }
                                    else if (intersectionType.Expressions.Count == 1)
                                    {
                                        // Adding to a union instead of returning the single expression's type directly to avoid issues with circular references.
                                        expression.ReturnType = new UnionType
                                        {
                                            Expressions = intersectionType.Expressions,
                                        };
                                        changed = true;
                                    }
                                    else if (intersectionType.Expressions.All(e => e.ReturnType is NumberRangeType))
                                    {
                                        var resultingType = intersectionType.Expressions
                                            .Select(e => (NumberRangeType)e.ReturnType)
                                            .Aggregate((a, b) => (NumberRangeType)a.IntersectWith(b));
                                        expression.ReturnType = resultingType;
                                        changed = true;
                                    }
                                    else
                                    {
                                        if (intersectionType.Expressions.Select(e => e.ReturnType).Distinct().Take(2).Count() == 1)
                                        {
                                            var returnType = intersectionType.Expressions.First().ReturnType;
                                            if (!(returnType is EnumType))
                                            {
                                                expression.ReturnType = returnType;
                                                changed = true;
                                            }
                                            else
                                            {
                                                var enumMembers = intersectionType.Expressions.Where(e => e is ObjectInfo).ToList();
                                                switch (enumMembers.Count)
                                                {
                                                    case 0:
                                                        expression.ReturnType = returnType;
                                                        changed = true;
                                                        break;

                                                    case 1:
                                                        intersectionType.Expressions = ImmutableHashSet.Create(enumMembers[0]);
                                                        changed = true;
                                                        break;

                                                    default:
                                                        expression.ReturnType = NoneType.Instance;
                                                        changed = true;
                                                        break;
                                                }
                                            }
                                        }

                                        // ((X) ∩ (Y ∩ Z)) ⇔ (X ∩ Y ∩ Z)
                                        var nestedIntersections = intersectionType.Expressions.Where(e => e.ReturnType is IntersectionType).ToList();
                                        if (nestedIntersections.Any())
                                        {
                                            intersectionType.Expressions = intersectionType.Expressions
                                                .Union(nestedIntersections.SelectMany(e => ((IntersectionType)e.ReturnType).Expressions))
                                                .Except(nestedIntersections);
                                            changed = true;
                                        }

                                        // (X ∪ Y ∪ Z) ∩ (X ∪ Z) ⇔ (X ∪ Z)
                                        if (intersectionType.Expressions.All(e =>
                                            (e.ReturnType is ObjectType) ||
                                            (e.ReturnType is EnumType) ||
                                            (e.ReturnType is NumberRangeType) ||
                                            (e.ReturnType is UnionType unionType && unionType.Expressions.All(u => u.ReturnType is ObjectType || u.ReturnType is EnumType || u.ReturnType is NumberRangeType))))
                                        {
                                            var unions = intersectionType.Expressions
                                                    .Select(u => FlattenUnion(new[] { u }));
                                            var intersected = unions.Aggregate((a, b) => a.Intersect(b));
                                            expression.ReturnType = new UnionType
                                            {
                                                Expressions = intersected,
                                            };
                                            changed = true;
                                        }
                                    }
                                }

                                break;
                        }

                        switch (expression.ReturnType)
                        {
                            case UnionType unionType:
                                if (unionTypesCache.TryGetValue(unionType.Expressions, out var foundUnion))
                                {
                                    expression.ReturnType = unionType = foundUnion;
                                }
                                else
                                {
                                    unionTypesCache[unionType.Expressions] = unionType;
                                }

                                if (!changed &&
                                    expression is ArgumentInfo argument &&
                                    argument.Expression is RelationInfo relation &&
                                    relation.Arity == 1 &&
                                    unionType.Expressions.Count >= 2 &&
                                    unionType.Expressions.All(e => e is ObjectInfo && e.ReturnType is ObjectType))
                                {
                                    enumCandidates.Add((relation, unionType));
                                }

                                break;
                            case IntersectionType intersectionType:
                                if (intersectionTypesCache.TryGetValue(intersectionType.Expressions, out var foundIntersection))
                                {
                                    expression.ReturnType = intersectionType = foundIntersection;
                                }
                                else
                                {
                                    intersectionTypesCache[intersectionType.Expressions] = intersectionType;
                                }

                                break;
                        }
                    });

                var count = enumCandidates.Count;
                if (!changed && count > 0)
                {
                    var conflicts = new bool[count];
                    for (var i = 0; i < count; i++)
                    {
                        var iExpr = enumCandidates[i].unionType.Expressions;
                        for (var j = i + 1; j < count; j++)
                        {
                            var jExpr = enumCandidates[j].unionType.Expressions;
                            if (iExpr.Overlaps(jExpr))
                            {
                                if (jExpr.SetEquals(iExpr))
                                {
                                    // TODO: Pick a best one?
                                }

                                if (!iExpr.IsProperSubsetOf(jExpr))
                                {
                                    conflicts[i] = true;
                                }

                                if (!jExpr.IsProperSubsetOf(iExpr))
                                {
                                    conflicts[j] = true;
                                }
                            }
                        }
                    }

                    for (var i = 0; i < count; i++)
                    {
                        if (!conflicts[i])
                        {
                            (var relation, var unionType) = enumCandidates[i];
                            var arg = relation.Arguments[0];
                            arg.ReturnType = EnumType.Create(relation, unionType.Expressions.Cast<ObjectInfo>());
                            changed = true;
                        }
                    }
                }
            }
            while (changed);
        }

        private class TypeUsageWalker : SupportedExpressionsTreeWalker
        {
            private readonly ImmutableDictionary<(Constant, int), ConstantInfo> expressionTypes;
            private readonly ImmutableDictionary<Sentence, ImmutableDictionary<IndividualVariable, VariableInfo>> containedVariables;
            private ImmutableDictionary<IndividualVariable, VariableInfo> variableTypes;
            private VariableDirection variableDirection;

            public TypeUsageWalker(AssignedTypes assignedTypes)
            {
                this.expressionTypes = assignedTypes.ExpressionTypes;
                this.containedVariables = assignedTypes.VariableTypes;
            }

            private enum VariableDirection
            {
                None = 0,
                In,
                Out,
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                var relationInfo = (RelationInfo)this.expressionTypes[(implicitRelationalSentence.Relation, implicitRelationalSentence.Arguments.Count)];
                this.AddArgumentUsages(implicitRelationalSentence.Arguments, relationInfo);
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                var functionInfo = (FunctionInfo)this.expressionTypes[(implicitFunctionalTerm.Function, implicitFunctionalTerm.Arguments.Count)];
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
                    this.variableTypes = this.containedVariables[(Sentence)form];
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

                    case StateType structType:
                        structType.Relations = structType.Relations.Add(expressionInfo);
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
                        return this.expressionTypes[(constant, 0)];

                    case ImplicitFunctionalTerm implicitFunctionalTerm:
                        return this.expressionTypes[(implicitFunctionalTerm.Function, implicitFunctionalTerm.Arguments.Count)];

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
