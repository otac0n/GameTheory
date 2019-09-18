// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.Shared.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides enumeration for console renderers in an assembly.
    /// </summary>
    public sealed class ConsoleRendererCatalog
    {
        private readonly ImmutableList<Assembly> assemblies;
        private readonly Lazy<ImmutableDictionary<Type, Type>> map;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleRendererCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for console renderers.</param>
        public ConsoleRendererCatalog(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleRendererCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for console renderers.</param>
        public ConsoleRendererCatalog(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = (assemblies ?? throw new ArgumentNullException(nameof(assemblies))).ToImmutableList();
            this.map = new Lazy<ImmutableDictionary<Type, Type>>(() => (from a in this.assemblies
                                                                        from t in a.ExportedTypes
                                                                        where !t.IsAbstract
                                                                        where t.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0)
                                                                        from i in t.GetInterfaces()
                                                                        where i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IConsoleRenderer<>)
                                                                        let m = i.GetGenericArguments()[0]
                                                                        select new { t, m }).ToImmutableDictionary(x => x.m, x => x.t));
        }

        /// <summary>
        /// Gets the list of console renderers who are capable of playing the specified move type.
        /// </summary>
        /// <typeparam name="TMove">The type of moves to be played.</typeparam>
        /// <returns>A collection of supported console renderers.</returns>
        public IConsoleRenderer<TMove> CreateConsoleRenderer<TMove>()
            where TMove : IMove => (IConsoleRenderer<TMove>)this.CreateConsoleRenderer(typeof(TMove));

        /// <summary>
        /// Gets the list of console renderers who are capable of playing the specified move type.
        /// </summary>
        /// <param name="moveType">The type of moves to be played.</param>
        /// <returns>A collection of supported console renderers.</returns>
        public object CreateConsoleRenderer(Type moveType) => this.CreateConsoleRendererInternal(moveType);

        private object CreateConsoleRendererInternal(Type moveType)
        {
            if (!this.map.Value.TryGetValue(moveType, out var rendererType))
            {
                rendererType = typeof(ToStringConsoleRenderer<>).MakeGenericType(moveType);
            }

            return Activator.CreateInstance(rendererType);
        }
    }
}
