// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class SyntaxHelper
    {
        public static readonly PredefinedTypeSyntax ObjectType =
            SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(
                    SyntaxKind.ObjectKeyword));

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
    }
}
