// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    internal static class ConsoleRenderer
    {
        private static readonly IImmutableDictionary<Type, Type> DefaultMap = (from t in typeof(IConsoleRenderer<>).Assembly.ExportedTypes
                                                                               where !t.IsAbstract
                                                                               where t.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0)
                                                                               from i in t.GetInterfaces()
                                                                               where i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IConsoleRenderer<>)
                                                                               let m = i.GetGenericArguments()[0]
                                                                               select new { t, m }).ToImmutableDictionary(x => x.m, x => x.t);

        internal static IConsoleRenderer<TMove> Default<TMove>()
            where TMove : IMove
        {
            if (DefaultMap.TryGetValue(typeof(TMove), out var rendererType))
            {
                return (IConsoleRenderer<TMove>)Activator.CreateInstance(rendererType);
            }

            return new ToStringConsoleRenderer<TMove>();
        }
    }
}
