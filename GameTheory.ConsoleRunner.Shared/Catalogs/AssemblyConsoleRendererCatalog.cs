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
            ArgumentNullException.ThrowIfNull(assemblies);

            this.assemblies = assemblies.ToImmutableList();
        }

        /// <inheritdoc/>
        protected override IEnumerable<Type> GetConsoleRenderers(Type gameStateType, Type moveType)
        {
            var consoleRendererUnconstructed = typeof(IConsoleRenderer<,>);
            var consoleRendererInterface = consoleRendererUnconstructed.MakeGenericType(gameStateType, moveType);
            var consoleRendererInterfaceInfo = consoleRendererInterface.GetTypeInfo();

            foreach (var assembly in this.assemblies)
            {
                var candidateTypeParameters = new HashSet<Type> { gameStateType, moveType };
                if (moveType.IsConstructedGenericType && moveType.GenericTypeArguments.Length == 1)
                {
                    candidateTypeParameters.Add(moveType.GenericTypeArguments.Single());
                }

                foreach (var type in assembly.ExportedTypes)
                {
                    var typeInfo = type.GetTypeInfo();
                    if (consoleRendererInterfaceInfo.IsAssignableFrom(typeInfo))
                    {
                        yield return type;
                        continue;
                    }

                    if (!type.IsGenericTypeDefinition || typeInfo.IsAbstract)
                    {
                        continue;
                    }

                    var targetDefinitions = typeInfo
                        .ImplementedInterfaces
                        .Where(i => i == consoleRendererUnconstructed || (i.IsConstructedGenericType && i.GetGenericTypeDefinition() == consoleRendererUnconstructed));
                    if (targetDefinitions.Any())
                    {
                        var matches = ReflectionUtilities.FixGenericTypeConstraints(typeInfo.GenericTypeParameters, _ => candidateTypeParameters);
                        foreach (var match in matches)
                        {
                            var consoleRendererType = ReflectionUtilities.TryMakeGenericType(type, match);
                            if (consoleRendererType != null)
                            {
                                if (consoleRendererInterfaceInfo.IsAssignableFrom(consoleRendererType.GetTypeInfo()))
                                {
                                    yield return consoleRendererType;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
