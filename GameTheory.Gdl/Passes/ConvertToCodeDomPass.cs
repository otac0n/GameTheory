// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat.Expressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ConvertToCodeDomPass : CompilePass
    {
        /// <inheritdoc/>
        public override IList<string> BlockedByErrors => new[]
        {
            "GDL099", // TODO: Link to AssignNamesPass
        };

        /// <inheritdoc/>
        public override IList<string> ErrorsProduced => new[]
        {
            "GDL100", // TODO: Create constant, etc.
        };

        /// <inheritdoc/>
        public override void Run(CompileResult result)
        {
            new Runner(result).Run();
        }

        private class Runner
        {
            private readonly CompileResult result;

            public Runner(CompileResult result)
            {
                this.result = result;
            }

            public static bool RequiresRuntimeCheck(ExpressionType t)
            {
                switch (t)
                {
                    case StateType stateType:
                    case UnionType unionType:
                    case IntersectionType intersectionType:
                    case NumberRangeType numberRangeType:
                        return true;

                    default:
                        return false;
                }
            }

            public static TypeSyntax Reference(ExpressionType type)
            {
                switch (type)
                {
                    case BooleanType booleanType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));

                    case NumberType numberType:
                    case NumberRangeType numberRangeType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));

                    case EnumType enumType:
                        return SyntaxFactory.ParseTypeName(enumType.Name);

                    case FunctionType functionType:
                        return SyntaxFactory.ParseTypeName(functionType.Name);

                    case StateType stateType:
                    case ObjectType objectType:
                    case UnionType unionType:
                    case IntersectionType intersectionType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

                    case ExpressionType _ when type.BuiltInType != null:
                        return SyntaxFactory.ParseTypeName(type.BuiltInType.FullName);
                }

                throw new NotSupportedException();
            }

            public static ExpressionSyntax AllMembers(ExpressionType type)
            {
                switch (type)
                {
                    case NumberRangeType numberRangeType:
                        return SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("Enumerable"),
                                SyntaxFactory.IdentifierName("Range")))
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList<ArgumentSyntax>().AddRange(new ArgumentSyntax[]
                                    {
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal(numberRangeType.Start))),
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal(numberRangeType.End - numberRangeType.Start + 1))),
                                    })));

                    case EnumType enumType:
                        return SyntaxFactory.ParseTypeName(enumType.Name);

                    case UnionType unionType:
                        return unionType.Expressions
                            .Select<ExpressionInfo, ExpressionSyntax>(expr =>
                            {
                                switch (expr)
                                {
                                    case ObjectInfo objectInfo:
                                        return SyntaxFactory.ArrayCreationExpression(
                                            SyntaxFactory.ArrayType(
                                                Reference(unionType))
                                            .WithRankSpecifiers(
                                                SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                                                    SyntaxFactory.ArrayRankSpecifier(
                                                        SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                            SyntaxFactory.OmittedArraySizeExpression())))))
                                            .WithInitializer(
                                                SyntaxFactory.InitializerExpression(
                                                    SyntaxKind.ArrayInitializerExpression,
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        CreateObjectReference(objectInfo))));

                                    default:
                                        return AllMembers(expr.ReturnType);
                                }
                            })
                            .Aggregate((a, b) =>
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        a,
                                        SyntaxFactory.IdentifierName("Concat")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                SyntaxFactory.Argument(
                                                    b)))));
                }

                throw new NotSupportedException();
            }

            public void Run()
            {
                var allTypes = new List<ExpressionType>();
                var allExpressions = new List<ExpressionInfo>();
                ExpressionTypeVisitor.Visit(this.result.AssignedTypes, visitExpression: allExpressions.Add, visitType: allTypes.Add);

                var renderedTypes = allTypes.Where(t =>
                    t.BuiltInType == null &&
                    (!(t is ObjectType) || t is FunctionType) &&
                    (!RequiresRuntimeCheck(t) || t is StateType));
                var renderedExpressions = allExpressions.Where(e =>
                    !(e is VariableInfo) &&
                    !(e is FunctionInfo) &&
                    !(e is ObjectInfo objectInfo && (objectInfo.Value is int || objectInfo.ReturnType is EnumType)));

                var root = SyntaxFactory.CompilationUnit();

                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(this.result.Name))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.QualifiedName(
                                    SyntaxFactory.IdentifierName("System"),
                                    SyntaxFactory.IdentifierName("Collections")),
                                SyntaxFactory.IdentifierName("Generic"))),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.IdentifierName("System"),
                                SyntaxFactory.IdentifierName("Linq"))));

                var gameState = SyntaxFactory.ClassDeclaration("GameState")
                    .WithModifiers(
                        new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

                foreach (var type in renderedTypes)
                {
                    switch (type)
                    {
                        case EnumType enumType:
                            gameState = gameState.AddMembers(CreateEnumTypeDeclaration(enumType));
                            break;

                        case FunctionType functionType:
                            gameState = gameState.AddMembers(CreateFunctionTypeDeclaration(functionType));
                            break;

                        case StateType stateType:
                            gameState = gameState.AddMembers(CreateStateTypeDeclaration(stateType));
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }

                foreach (var expr in renderedExpressions)
                {
                    switch (expr)
                    {
                        case ObjectInfo objectInfo:
                            if (objectInfo.ReturnType is ObjectType && objectInfo.Value is string value)
                            {
                                gameState = gameState.AddMembers(CreateObjectDeclaration(objectInfo, value));
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }

                            break;

                        case RelationInfo relationInfo:
                            switch (relationInfo.Id)
                            {
                                case "INIT":
                                case "NEXT":
                                    break;

                                default:
                                    gameState = gameState.AddMembers(this.CreateRelationDeclaration(relationInfo));
                                    break;
                            }

                            break;

                        case LogicalInfo logicalInfo:
                            gameState = gameState.AddMembers(this.CreateLogicalDeclaration(logicalInfo));
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }

                ns = ns.AddMembers(gameState);
                root = root.AddMembers(ns);
                this.result.DeclarationSyntax = root.NormalizeWhitespace();
            }

            private static ExpressionSyntax ObjectEqualsExpression(ExpressionSyntax left, ExpressionSyntax right) =>
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.PredefinedType(
                            SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                        SyntaxFactory.IdentifierName("Equals")),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(new ArgumentSyntax[]
                            {
                                SyntaxFactory.Argument(left),
                                SyntaxFactory.Argument(right),
                            })));

            private static FieldDeclarationSyntax CreateObjectDeclaration(ObjectInfo objectInfo, string value) =>
                SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        Reference(objectInfo.ReturnType),
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(objectInfo.Id))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value)))))))
                    .WithModifiers(new SyntaxTokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)));

            private static ExpressionSyntax CreateObjectReference(ObjectInfo objectInfo)
            {
                switch (objectInfo.ReturnType)
                {
                    case EnumType enumType:
                        return SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(enumType.Name),
                            SyntaxFactory.IdentifierName(objectInfo.Id));

                    case NumberType numberType when objectInfo.Value is int:
                        return SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((int)objectInfo.Value));

                    default:
                        return SyntaxFactory.IdentifierName(objectInfo.Id);
                }
            }

            private MemberDeclarationSyntax CreateLogicalDeclaration(LogicalInfo logicalInfo) =>
                this.CreateLogicalFunctionDeclaration(logicalInfo, Array.Empty<ArgumentInfo>(), logicalInfo.Body);

            private MemberDeclarationSyntax CreateRelationDeclaration(RelationInfo relationInfo) =>
                this.CreateLogicalFunctionDeclaration(relationInfo, relationInfo.Arguments, relationInfo.Body);

            private MemberDeclarationSyntax CreateLogicalFunctionDeclaration(ExpressionInfo expression, ArgumentInfo[] parameters, IEnumerable<Sentence> sentences)
            {
                var methodElement = SyntaxFactory.MethodDeclaration(
                    Reference(expression.ReturnType),
                    expression.Id)
                    .WithModifiers(new SyntaxTokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

                var parameterNames = parameters.ToDictionary(p => p, p => p.Id.TrimStart('?')); // TODO: Better name resolution.

                foreach (var param in parameters)
                {
                    methodElement = methodElement.AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterNames[param]))
                        .WithType(Reference(param.ReturnType)));
                }

                var returnTrue = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));

                foreach (var sentence in sentences)
                {
                    var implicated = sentence.GetImplicatedSentence();

                    var scope = ImmutableDictionary<IndividualVariable, ExpressionSyntax>.Empty;
                    var walker = new ScopeWalker(this.result, parameters, parameterNames, scope, this.ConvertExpression);
                    walker.Walk((Expression)implicated);
                    var declarations = walker.Declarations;
                    var parameterEquality = walker.ParameterEquality;
                    scope = walker.Scope;

                    var sentenceVariables = this.result.AssignedTypes.VariableTypes[sentence].ToDictionary(v => v.Item1, v => v.Item2);

                    var conditions = sentence is Implication implication
                        ? implication.Antecedents
                        : ImmutableList<Sentence>.Empty;

                    StatementSyntax GetStatement(int i, ImmutableDictionary<IndividualVariable, ExpressionSyntax>  s1)
                    {
                        if (i >= conditions.Count)
                        {
                            return returnTrue;
                        }

                        return this.ConvertSentence(conditions[i], s2 => GetStatement(i + 1, s2), sentenceVariables, s1);
                    }

                    var root = GetStatement(0, scope);

                    if (parameterEquality.Count > 0)
                    {
                        root = SyntaxFactory.IfStatement(
                            parameterEquality.Aggregate((left, right) =>
                                SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, left, right)),
                            SyntaxFactory.Block(root));
                    }

                    root = SyntaxFactory.Block(declarations)
                        .AddStatements(root)
                        .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {sentence}")));

                    methodElement = methodElement.AddBodyStatements(root);
                }

                methodElement = methodElement.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));

                return methodElement;
            }

            private StatementSyntax ConvertSentence(Sentence sentence, Func<ImmutableDictionary<IndividualVariable, ExpressionSyntax>, StatementSyntax> inner, Dictionary<IndividualVariable, VariableInfo> sentenceVariables, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope)
            {
                var variables = this.result.ContainedVariables[sentence].Except(scope.Keys);
                if (variables.Count > 0)
                {
                    var variable = (IndividualVariable)variables.First();
                    var variableInfo = sentenceVariables[variable];
                    var name = variable.Name.TrimStart('?'); // TODO: Avoid conflicts.
                    scope = scope.SetItem(variable, SyntaxFactory.IdentifierName(name));

                    return SyntaxFactory.ForEachStatement(
                        Reference(variableInfo.ReturnType),
                        SyntaxFactory.Identifier(name),
                        AllMembers(variableInfo.ReturnType),
                        SyntaxFactory.Block(
                            this.ConvertSentence(sentence, inner, sentenceVariables, scope)));
                }
                else
                {
                    return SyntaxFactory.IfStatement(
                        this.ConvertCondition(sentence, scope),
                        SyntaxFactory.Block(inner(scope)))
                    .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {sentence}")));
                }
            }

            private ExpressionSyntax ConvertCondition(Sentence condition, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope)
            {
                switch (condition)
                {
                    case ImplicitRelationalSentence implicitRelationalSentence:
                        return this.ConvertImplicitRelationalCondition(implicitRelationalSentence, scope);

                    case ConstantSentence constantSentence:
                        return this.ConvertLogicalCondition(constantSentence);

                    case Negation negation:
                        return this.ConvertNegationCondition(negation, scope);

                    default:
                        throw new NotSupportedException();
                }
            }

            private ExpressionSyntax ConvertExpression(Term term, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope)
            {
                switch (term)
                {
                    case ImplicitFunctionalTerm implicitFunctionalTerm:
                        return this.ConvertFunctionalTermExpression(implicitFunctionalTerm, scope);

                    case IndividualVariable individualVariable:
                        return this.ConvertVariableExpression(individualVariable, scope);

                    case Constant constant:
                        return this.ConvertConstantExpression(constant);

                    default:
                        throw new NotSupportedException();
                }
            }

            private ExpressionSyntax ConvertConstantExpression(Constant constant) =>
                CreateObjectReference((ObjectInfo)this.result.AssignedTypes.ExpressionTypes[(constant.Id, 0)]);

            private ExpressionSyntax ConvertVariableExpression(IndividualVariable individualVariable, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope) =>
                scope[individualVariable];

            private ExpressionSyntax ConvertFunctionalTermExpression(ImplicitFunctionalTerm implicitFunctionalTerm, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope) =>
                SyntaxFactory.ObjectCreationExpression( // TODO: Runtime type checks
                    SyntaxFactory.IdentifierName(implicitFunctionalTerm.Function.Id))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(implicitFunctionalTerm.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));

            private ExpressionSyntax ConvertLogicalCondition(ConstantSentence constantSentence) =>
                SyntaxFactory.InvocationExpression( // TODO: Runtime type checks
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(constantSentence.Constant.Id)));

            private ExpressionSyntax ConvertNegationCondition(Negation negation, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope) =>
                SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    this.ConvertCondition(negation.Negated, scope));

            private ExpressionSyntax ConvertImplicitRelationalCondition(ImplicitRelationalSentence implicitRelationalSentence, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope) =>
                SyntaxFactory.InvocationExpression( // TODO: Runtime type checks
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(implicitRelationalSentence.Relation.Id)),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(implicitRelationalSentence.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));

            private static EnumDeclarationSyntax CreateEnumTypeDeclaration(EnumType enumType)
            {
                var enumElement = SyntaxFactory.EnumDeclaration(enumType.Name)
                    .WithModifiers(
                        new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

                foreach (var obj in enumType.Objects)
                {
                    enumElement = enumElement.AddMembers(
                        SyntaxFactory.EnumMemberDeclaration(obj.Id));
                }

                return enumElement;
            }

            private static StructDeclarationSyntax CreateFunctionTypeDeclaration(FunctionType functionType)
            {
                var structElement = SyntaxFactory.StructDeclaration(functionType.Name)
                    .WithModifiers(
                        new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

                var constructor = SyntaxFactory.ConstructorDeclaration(structElement.Identifier)
                    .WithModifiers(
                        new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

                foreach (var arg in functionType.FunctionInfo.Arguments)
                {
                    var type = Reference(arg.ReturnType);
                    var fieldVariable = SyntaxFactory.VariableDeclarator("_" + arg.Id.TrimStart('?')); // TODO: Better name resolution.
                    var fieldElement = SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(
                            type,
                            SyntaxFactory.SingletonSeparatedList(fieldVariable)))
                        .WithModifiers(
                            new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

                    var parameter = SyntaxFactory.Parameter(fieldVariable.Identifier).WithType(type);
                    constructor = constructor.AddParameterListParameters(parameter);

                    constructor = constructor.AddBodyStatements(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ThisExpression(),
                                SyntaxFactory.IdentifierName(fieldVariable.Identifier)),
                            SyntaxFactory.IdentifierName(parameter.Identifier))));

                    var propElement = SyntaxFactory.PropertyDeclaration(type, arg.Id.TrimStart('?')) // TODO: Better name resolution.
                        .WithModifiers(
                            new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                SyntaxFactory.Block(
                                    SyntaxFactory.ReturnStatement(SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName(fieldVariable.Identifier))))));

                    structElement = structElement.AddMembers(fieldElement, propElement);
                }

                structElement = structElement.AddMembers(constructor);

                return structElement;
            }

            private static ClassDeclarationSyntax CreateStateTypeDeclaration(StateType stateType)
            {
                var classElement = SyntaxFactory.ClassDeclaration(stateType.Name);

                var constructor = SyntaxFactory.ConstructorDeclaration(classElement.Identifier)
                    .WithBody(SyntaxFactory.Block())
                    .WithModifiers(
                        new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

                foreach (var obj in stateType.Relations)
                {
                    constructor = constructor.AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier(obj.Id)).WithType(
                                SyntaxFactory.GenericName(
                                    SyntaxFactory.Identifier("IEnumerable"),
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(Reference(obj.ReturnType))))));
                }

                classElement = classElement.AddMembers(constructor);

                return classElement;
            }

            private class ScopeWalker : SupportedExpressionsTreeWalker
            {
                private readonly CompileResult result;
                private readonly ArgumentInfo[] parameters;
                private readonly Dictionary<ArgumentInfo, string> parameterNames;
                private readonly Func<Term, ImmutableDictionary<IndividualVariable, ExpressionSyntax>, ExpressionSyntax> convertExpression;
                private ArgumentInfo param;
                private ExpressionSyntax path;

                public ScopeWalker(CompileResult result, ArgumentInfo[] parameters, Dictionary<ArgumentInfo, string> parameterNames, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope, Func<Term, ImmutableDictionary<IndividualVariable, ExpressionSyntax>, ExpressionSyntax> convertExpression)
                {
                    this.result = result;
                    this.parameters = parameters;
                    this.parameterNames = parameterNames;
                    this.convertExpression = convertExpression;
                    this.Declarations = new List<StatementSyntax>();
                    this.ParameterEquality = new List<ExpressionSyntax>();
                    this.Scope = scope;
                }

                public List<StatementSyntax> Declarations { get; }

                public List<ExpressionSyntax> ParameterEquality { get; }

                public ImmutableDictionary<IndividualVariable, ExpressionSyntax> Scope { get; set; }

                public override void Walk(ConstantSentence constantSentence)
                {
                    Debug.Assert(this.parameters.Length == 0, "Arguments' arity doesn't match parameters' arity.");
                }

                public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
                {
                    var args = implicitRelationalSentence.Arguments;
                    Debug.Assert(this.parameters.Length == args.Count, "Arguments' arity doesn't match parameters' arity.");

                    for (var i = 0; i < args.Count; i++)
                    {
                        this.param = this.parameters[i];
                        this.path = SyntaxFactory.ParseName(this.parameterNames[this.param]);
                        this.Walk((Expression)args[i]);
                    }
                }

                public override void Walk(Term term)
                {
                    if (term is IndividualVariable argVar)
                    {
                        if (this.Scope.TryGetValue(argVar, out var matching))
                        {
                            this.ParameterEquality.Add(ObjectEqualsExpression(this.path, matching));
                        }
                        else
                        {
                            this.Scope = this.Scope.SetItem(argVar, this.path);
                        }
                    }
                    else
                    {
                        if (this.result.ContainedVariables[term].Count == 0)
                        {
                            this.ParameterEquality.Add(ObjectEqualsExpression(this.path, this.convertExpression(term, this.Scope)));
                        }
                        else
                        {
                            var arg = this.result.AssignedTypes.GetExpressionInfo(term);

                            // TODO: Allow arg to be a larger type than param. There's no iheritance. Will this matter?
                            if (this.param.ReturnType != arg.ReturnType)
                            {
                                var name = $"as{arg.ReturnType}"; // TODO: Better name resolution.
                                var typedVar = SyntaxFactory.IdentifierName(name);
                                this.Declarations.Add(
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(
                                            Reference(arg.ReturnType))
                                        .WithVariables(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.VariableDeclarator(
                                                    SyntaxFactory.Identifier(name))))));
                                this.ParameterEquality.Add(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.LogicalAndExpression,
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.IsExpression,
                                            this.path,
                                            Reference(arg.ReturnType)),
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.IsExpression,
                                            SyntaxFactory.ParenthesizedExpression(
                                                SyntaxFactory.AssignmentExpression(
                                                    SyntaxKind.SimpleAssignmentExpression,
                                                    typedVar,
                                                    SyntaxFactory.CastExpression(
                                                        Reference(arg.ReturnType),
                                                        this.path))),
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))));
                                this.path = typedVar;
                            }

                            base.Walk(term);
                        }
                    }
                }

                public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
                {
                    var args = implicitFunctionalTerm.Arguments;
                    var functionInfo = (FunctionInfo)this.result.AssignedTypes.GetExpressionInfo(implicitFunctionalTerm);

                    for (var i = 0; i < args.Count; i++)
                    {
                        var originalPath = this.path;

                        this.path = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            this.path,
                            SyntaxFactory.IdentifierName(functionInfo.Arguments[i].Id.TrimStart('?'))); // TODO: Lookup.

                        this.Walk((Expression)args[i]);

                        this.path = originalPath;
                    }
                }
            }
        }
    }
}
