// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Implements <see cref="IComparer{T}"/> for <see cref="Enum">enums</see> without incurring boxing overhead.
    /// </summary>
    /// <typeparam name="T">The enum type to compare.</typeparam>
    /// <remarks>
    /// Use <see cref="EqualityComparer{T}"/> for eqaulity comparison.
    /// </remarks>
    public sealed class EnumComparer<T> : IComparer<T>
    {
        /// <summary>
        /// The singleton instance of the comparer.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Intended to match the API contract of EqualityComparer<T>.Default.")]
        public static readonly EnumComparer<T> Default = new EnumComparer<T>();

        private static Func<T, T, int> compare;

        static EnumComparer()
        {
            var enumType = typeof(T);
            var underlyingType = Enum.GetUnderlyingType(enumType);
            var compareMethod = underlyingType
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(IComparable.CompareTo))
                .Where(m => m.GetParameters().First().ParameterType == underlyingType)
                .First();

            var left = Expression.Parameter(enumType);
            var right = Expression.Parameter(enumType);
            var lambda = Expression.Lambda<Func<T, T, int>>(
                Expression.Call(Expression.Convert(left, underlyingType), compareMethod, Expression.Convert(right, underlyingType)),
                left,
                right);
            compare = lambda.Compile();
        }

        /// <inheritdoc/>
        public int Compare(T x, T y) => compare(x, y);
    }
}
