// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using GameTheory.Games.Ergo.Cards;

    internal static class Compiler
    {
        private static readonly ParameterExpression A = Expression.Variable(typeof(bool), "a");
        private static readonly ParameterExpression B = Expression.Variable(typeof(bool), "b");
        private static readonly ParameterExpression C = Expression.Variable(typeof(bool), "c");
        private static readonly ParameterExpression D = Expression.Variable(typeof(bool), "d");
        private static readonly ImmutableList<Symbol> Precedence = ImmutableList.Create(Symbol.Then, Symbol.Or, Symbol.And, Symbol.LeftParenthesis);

        private static readonly ConstantExpression True = Expression.Constant(true);

        private static readonly ImmutableDictionary<Symbol, ParameterExpression> Variables = ImmutableDictionary.CreateRange(new Dictionary<Symbol, ParameterExpression>
        {
            [Symbol.A] = A,
            [Symbol.B] = B,
            [Symbol.C] = C,
            [Symbol.D] = D,
        });

        public static Expression CompilePremise(IEnumerable<Symbol> proof)
        {
            var output = new Stack<Expression>();
            var operators = new Stack<Symbol>();

            var not = new Func<Expression, Expression>(expr => expr.NodeType == ExpressionType.Not
                ? ((UnaryExpression)expr).Operand
                : Expression.Not(expr));

            var clear = new Action(() =>
            {
                while (operators.Count > 0 && Precedence.IndexOf(operators.Peek()) < 0)
                {
                    Debug.Assert(operators.Pop() == Symbol.Not, "Not impemented.");
                    output.Push(not(output.Pop()));
                }
            });

            var push = new Action<Expression>(expr =>
            {
                output.Push(expr);
                clear();
            });

            var popOperator = new Action(() =>
            {
                var right = output.Pop();
                var left = output.Pop();

                Expression result;
                var op = operators.Pop();
                switch (op)
                {
                    case Symbol.And:
                        result = Expression.AndAlso(left, right);
                        break;

                    case Symbol.Or:
                        result = Expression.OrElse(left, right);
                        break;

                    case Symbol.Then:
                        result = Expression.OrElse(not(left), right);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                output.Push(result);
            });

            foreach (var symbol in proof)
            {
                switch (symbol)
                {
                    case Symbol.A:
                    case Symbol.B:
                    case Symbol.C:
                    case Symbol.D:
                        push(Variables[symbol]);
                        break;

                    case Symbol.And:
                    case Symbol.Or:
                    case Symbol.Then:
                        while (operators.Count > 0)
                        {
                            var precedence = Precedence.IndexOf(symbol);
                            var previousPrecedence = Precedence.IndexOf(operators.Peek());
                            if (precedence < previousPrecedence)
                            {
                                break;
                            }

                            popOperator();
                        }

                        operators.Push(symbol);
                        break;

                    case Symbol.Not:
                    case Symbol.LeftParenthesis:
                        operators.Push(symbol);
                        break;

                    case Symbol.RightParenthesis:
                        while (operators.Peek() != Symbol.LeftParenthesis)
                        {
                            popOperator();
                        }

                        operators.Pop();
                        clear();

                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            while (operators.Count > 0)
            {
                popOperator();
            }

            return output.SingleOrDefault() ?? True;
        }

        public static Expression<Func<bool, bool, bool, bool, bool>> CompileProof(ImmutableList<ImmutableList<PlacedCard>> proof)
        {
            var body = proof
                .Select(p => CompilePremise(p.Select(c => c.Symbol)))
                .Aggregate((a, b) => Expression.AndAlso(a, b));

            return Expression.Lambda<Func<bool, bool, bool, bool, bool>>(body, A, B, C, D);
        }

        public static bool IsValid(ImmutableList<ImmutableList<PlacedCard>> proof)
        {
            return proof.All(p => IsValid(p.Select(c => c.Symbol)));
        }

        public static bool IsValid(IEnumerable<Symbol> symbols)
        {
            const int Var = 0b0000001;
            const int Op = 0b0000010;
            const int Not = 0b0000100;
            const int Open = 0b001000;
            const int Close = 0b0100000;
            const int End = 0b1000000;

            var parens = 0;
            var st = Var | Not | Open | End;

            foreach (var symbol in symbols)
            {
                switch (symbol)
                {
                    case Symbol.A:
                    case Symbol.B:
                    case Symbol.C:
                    case Symbol.D:
                        if ((st & Var) == 0)
                        {
                            return false;
                        }

                        st = Op | Close | End;
                        break;

                    case Symbol.And:
                    case Symbol.Or:
                    case Symbol.Then:
                        if ((st & Op) == 0)
                        {
                            return false;
                        }

                        st = Var | Not | Open;
                        break;

                    case Symbol.Not:
                        if ((st & Not) == 0)
                        {
                            return false;
                        }

                        st = Var | Open;
                        break;

                    case Symbol.LeftParenthesis:
                        if ((st & Open) == 0)
                        {
                            return false;
                        }

                        parens += 1;

                        st = Var | Not | Open;
                        break;

                    case Symbol.RightParenthesis:
                        if ((st & Close) == 0)
                        {
                            return false;
                        }

                        parens -= 1;
                        if (parens < 0)
                        {
                            return false;
                        }

                        st = Op | Close | End;
                        break;

                    default:
                        return false;
                }
            }

            if ((st & End) == 0)
            {
                return false;
            }

            if (parens != 0)
            {
                return false;
            }

            return true;
        }

        public static bool TryCompileProof(ImmutableList<ImmutableList<PlacedCard>> proof, out Expression<Func<bool, bool, bool, bool, bool>> compiled)
        {
            try
            {
                compiled = CompileProof(proof);
                return true;
            }
            catch (InvalidOperationException)
            {
                compiled = null;
                return false;
            }
        }
    }
}
