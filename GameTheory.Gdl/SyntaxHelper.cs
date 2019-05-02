// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class SyntaxHelper
    {
        public static readonly Comparer<MemberDeclarationSyntax> MemberOrder = Comparer<MemberDeclarationSyntax>.Create((a, b) =>
        {
            var comp = 0;
            if ((comp = (b is FieldDeclarationSyntax).CompareTo(a is FieldDeclarationSyntax)) != 0 ||
                (comp = (b is ConstructorDeclarationSyntax).CompareTo(a is ConstructorDeclarationSyntax)) != 0 ||
                (comp = (b is DestructorDeclarationSyntax).CompareTo(a is DestructorDeclarationSyntax)) != 0 ||
                (comp = (b is DelegateDeclarationSyntax).CompareTo(a is DelegateDeclarationSyntax)) != 0 ||
                (comp = (b is EventDeclarationSyntax).CompareTo(a is EventDeclarationSyntax)) != 0 ||
                (comp = (b is EnumDeclarationSyntax).CompareTo(a is EnumDeclarationSyntax)) != 0 ||
                (comp = (b is InterfaceDeclarationSyntax).CompareTo(a is InterfaceDeclarationSyntax)) != 0 ||
                (comp = (b is PropertyDeclarationSyntax).CompareTo(a is PropertyDeclarationSyntax)) != 0 ||
                (comp = (b is IndexerDeclarationSyntax).CompareTo(a is IndexerDeclarationSyntax)) != 0 ||
                (comp = (b is MethodDeclarationSyntax).CompareTo(a is MethodDeclarationSyntax)) != 0 ||
                (comp = (b is StructDeclarationSyntax).CompareTo(a is StructDeclarationSyntax)) != 0 ||
                (comp = (b is ClassDeclarationSyntax).CompareTo(a is ClassDeclarationSyntax)) != 0)
            {
                return comp;
            }

            if (a is BaseFieldDeclarationSyntax aField && b is BaseFieldDeclarationSyntax bField)
            {
                if ((comp = bField.Modifiers.Any(SyntaxKind.ConstKeyword).CompareTo(aField.Modifiers.Any(SyntaxKind.ConstKeyword))) != 0 ||
                    (comp = bField.Modifiers.Any(SyntaxKind.PublicKeyword).CompareTo(aField.Modifiers.Any(SyntaxKind.PublicKeyword))) != 0 ||
                    (comp = bField.Modifiers.Any(SyntaxKind.InternalKeyword).CompareTo(aField.Modifiers.Any(SyntaxKind.InternalKeyword))) != 0 ||
                    (comp = aField.Modifiers.Any(SyntaxKind.PrivateKeyword).CompareTo(bField.Modifiers.Any(SyntaxKind.PrivateKeyword))) != 0 ||
                    (comp = aField.Modifiers.Any(SyntaxKind.ProtectedKeyword).CompareTo(bField.Modifiers.Any(SyntaxKind.ProtectedKeyword))) != 0 ||
                    (comp = bField.Modifiers.Any(SyntaxKind.StaticKeyword).CompareTo(aField.Modifiers.Any(SyntaxKind.StaticKeyword))) != 0 ||
                    (comp = bField.Modifiers.Any(SyntaxKind.ReadOnlyKeyword).CompareTo(aField.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))) != 0)
                {
                    return comp;
                }

                if ((comp = aField.Declaration.Variables.Count.CompareTo(bField.Declaration.Variables.Count)) != 0 ||
                    (comp = aField.Declaration.Variables.First().Identifier.Text.CompareTo(bField.Declaration.Variables.First().Identifier.Text)) != 0)
                {
                    return comp;
                }
            }
            else if (a is BasePropertyDeclarationSyntax aProp && b is BasePropertyDeclarationSyntax bProp)
            {
                if ((comp = bProp.Modifiers.Any(SyntaxKind.PublicKeyword).CompareTo(aProp.Modifiers.Any(SyntaxKind.PublicKeyword))) != 0 ||
                    (comp = bProp.Modifiers.Any(SyntaxKind.InternalKeyword).CompareTo(aProp.Modifiers.Any(SyntaxKind.InternalKeyword))) != 0 ||
                    (comp = aProp.Modifiers.Any(SyntaxKind.PrivateKeyword).CompareTo(bProp.Modifiers.Any(SyntaxKind.PrivateKeyword))) != 0 ||
                    (comp = aProp.Modifiers.Any(SyntaxKind.ProtectedKeyword).CompareTo(bProp.Modifiers.Any(SyntaxKind.ProtectedKeyword))) != 0 ||
                    (comp = bProp.Modifiers.Any(SyntaxKind.StaticKeyword).CompareTo(aProp.Modifiers.Any(SyntaxKind.StaticKeyword))) != 0)
                {
                    return comp;
                }

                if (a is PropertyDeclarationSyntax aProperty && b is PropertyDeclarationSyntax bProperty)
                {
                    if ((comp = aProperty.Identifier.Text.CompareTo(bProperty.Identifier.Text)) != 0)
                    {
                        return comp;
                    }
                }
            }
            else if (a is BaseMethodDeclarationSyntax aMethod && b is BaseMethodDeclarationSyntax bMethod)
            {
                if ((comp = bMethod.Modifiers.Any(SyntaxKind.PublicKeyword).CompareTo(aMethod.Modifiers.Any(SyntaxKind.PublicKeyword))) != 0 ||
                    (comp = bMethod.Modifiers.Any(SyntaxKind.InternalKeyword).CompareTo(aMethod.Modifiers.Any(SyntaxKind.InternalKeyword))) != 0 ||
                    (comp = aMethod.Modifiers.Any(SyntaxKind.PrivateKeyword).CompareTo(bMethod.Modifiers.Any(SyntaxKind.PrivateKeyword))) != 0 ||
                    (comp = aMethod.Modifiers.Any(SyntaxKind.ProtectedKeyword).CompareTo(bMethod.Modifiers.Any(SyntaxKind.ProtectedKeyword))) != 0 ||
                    (comp = bMethod.Modifiers.Any(SyntaxKind.StaticKeyword).CompareTo(aMethod.Modifiers.Any(SyntaxKind.StaticKeyword))) != 0 ||
                    (comp = aMethod.Modifiers.Any(SyntaxKind.StaticKeyword).CompareTo(aMethod.Modifiers.Any(SyntaxKind.StaticKeyword))) != 0)
                {
                    return comp;
                }

                if (a is MethodDeclarationSyntax aMethodDeclaration && b is MethodDeclarationSyntax bMethodDeclaration)
                {
                    if ((comp = aMethodDeclaration.Identifier.Text.CompareTo(bMethodDeclaration.Identifier.Text)) != 0)
                    {
                        return comp;
                    }
                }
            }
            else if (a is BaseTypeDeclarationSyntax aType && b is BaseTypeDeclarationSyntax bType)
            {
                if ((comp = bType.Modifiers.Any(SyntaxKind.PublicKeyword).CompareTo(aType.Modifiers.Any(SyntaxKind.PublicKeyword))) != 0 ||
                    (comp = bType.Modifiers.Any(SyntaxKind.InternalKeyword).CompareTo(aType.Modifiers.Any(SyntaxKind.InternalKeyword))) != 0 ||
                    (comp = aType.Modifiers.Any(SyntaxKind.PrivateKeyword).CompareTo(bType.Modifiers.Any(SyntaxKind.PrivateKeyword))) != 0 ||
                    (comp = aType.Modifiers.Any(SyntaxKind.ProtectedKeyword).CompareTo(bType.Modifiers.Any(SyntaxKind.ProtectedKeyword))) != 0 ||
                    (comp = bType.Modifiers.Any(SyntaxKind.StaticKeyword).CompareTo(aType.Modifiers.Any(SyntaxKind.StaticKeyword))) != 0 ||
                    (comp = aType.Identifier.Text.CompareTo(bType.Identifier.Text)) != 0)
                {
                    return comp;
                }
            }

            return comp;
        });

        public static readonly PredefinedTypeSyntax BoolType =
            SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(
                    SyntaxKind.BoolKeyword));

        public static readonly PredefinedTypeSyntax IntType =
            SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(
                    SyntaxKind.IntKeyword));

        public static readonly PredefinedTypeSyntax ObjectType =
            SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(
                    SyntaxKind.ObjectKeyword));

        public static readonly PredefinedTypeSyntax StringType =
            SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(
                    SyntaxKind.StringKeyword));

        public static readonly PredefinedTypeSyntax VoidType =
            SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(
                    SyntaxKind.VoidKeyword));

        public static readonly LiteralExpressionSyntax Null =
            SyntaxFactory.LiteralExpression(
                SyntaxKind.NullLiteralExpression);

        public static readonly LiteralExpressionSyntax True =
            SyntaxFactory.LiteralExpression(
                SyntaxKind.TrueLiteralExpression);

        public static readonly LiteralExpressionSyntax False =
            SyntaxFactory.LiteralExpression(
                SyntaxKind.FalseLiteralExpression);

        public static ArrayTypeSyntax ArrayType(TypeSyntax elementType, params ExpressionSyntax[] sizes) =>
            SyntaxFactory.ArrayType(
                elementType)
            .AddRankSpecifiers(
                SyntaxFactory.ArrayRankSpecifier()
                    .AddSizes(sizes));

        public static BinaryExpressionSyntax Coalesce(ExpressionSyntax left, ExpressionSyntax right) =>
            SyntaxFactory.BinaryExpression(
                SyntaxKind.CoalesceExpression,
                left,
                right);

        public static StatementSyntax AssignmentStatement(ExpressionSyntax left, ExpressionSyntax right) =>
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    left,
                    right));

        public static ExpressionSyntax ObjectEqualsExpression(ExpressionSyntax left, ExpressionSyntax right) =>
            SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxHelper.ObjectType,
                    SyntaxFactory.IdentifierName(nameof(object.Equals))),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList<ArgumentSyntax>()
                        .AddRange(new ArgumentSyntax[]
                        {
                                SyntaxFactory.Argument(left),
                                SyntaxFactory.Argument(right),
                        })));

        public static ExpressionSyntax EnumerableRangeExpression(int start, int count) =>
            SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(nameof(Enumerable)),
                    SyntaxFactory.IdentifierName(nameof(Enumerable.Range))))
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(
                        SyntaxHelper.LiteralExpression(start)),
                    SyntaxFactory.Argument(
                        SyntaxHelper.LiteralExpression(count)));

        public static LiteralExpressionSyntax LiteralExpression(int value) =>
            SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(value));

        public static LiteralExpressionSyntax LiteralExpression(string value) =>
            SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(value));

        public static T ReorderMembers<T>(T type)
            where T : TypeDeclarationSyntax
        {
            var members = type.Members
                .Select(m => m is TypeDeclarationSyntax typeDeclarationSyntax ? ReorderMembers(typeDeclarationSyntax) : m)
                .OrderBy(m => m, SyntaxHelper.MemberOrder);
            return (T)type.WithMembers(SyntaxFactory.List(members));
        }
    }
}
