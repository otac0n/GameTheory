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
    public class AssemblyConsoleRendererCatalog : ConsoleRendererCatalogBase
    {
        private readonly ImmutableList<Assembly> assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyConsoleRendererCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for console renderers.</param>
        public AssemblyConsoleRendererCatalog(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyConsoleRendererCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for console renderers.</param>
        public AssemblyConsoleRendererCatalog(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = (assemblies ?? throw new ArgumentNullException(nameof(assemblies))).ToImmutableList();
        }

        /// <inheritdoc/>
        protected override IEnumerable<Type> GetConsoleRenderers(Type moveType)
        {
            var consoleRendererUnconstructed = typeof(IConsoleRenderer<>);
            var consoleRendererInterface = consoleRendererUnconstructed.MakeGenericType(moveType);
            foreach (var assembly in this.assemblies)
            {
                var staticConsoleRenderers = from t in assembly.ExportedTypes
                                             where consoleRendererInterface.IsAssignableFrom(t)
                                             select t;

                var candidateTypeParameters = new List<Type> { moveType };
                if (moveType.IsConstructedGenericType && moveType.GenericTypeArguments.Length == 1)
                {
                    candidateTypeParameters.Add(moveType.GenericTypeArguments.Single());
                }

                var tryMakeConsoleRendererType = new Func<Type, Type, Type>((consoleRendererType, argument) =>
                {
                    try
                    {
                        return consoleRendererType.MakeGenericType(argument);
                    }
                    catch (ArgumentException)
                    {
                        return null;
                    }
                });

                var constructedConsoleRenderers = from t in assembly.ExportedTypes
                                                  let info = t.GetTypeInfo()
                                                  let typeParameters = info.GenericTypeParameters
                                                  where typeParameters.Length == 1
                                                  where info.ImplementedInterfaces.Any(i => i == consoleRendererUnconstructed || (i.IsConstructedGenericType && i.GetGenericTypeDefinition() == consoleRendererUnconstructed))
                                                  let constraints = typeParameters[0].GetGenericParameterConstraints()
                                                  from candidate in candidateTypeParameters
                                                  let c = tryMakeConsoleRendererType(t, candidate)
                                                  where c != null && !c.IsAbstract
                                                  where consoleRendererInterface.IsAssignableFrom(c)
                                                  select c;

                var consoleRendererTypes = Enumerable.Concat(
                    staticConsoleRenderers,
                    constructedConsoleRenderers);
                foreach (var consoleRendererType in consoleRendererTypes)
                {
                    yield return consoleRendererType;
                }
            }
        }
    }
}
