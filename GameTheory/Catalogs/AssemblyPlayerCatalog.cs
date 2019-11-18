// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Exposes the players available as exported types from a list of assemblies.
    /// </summary>
    public class AssemblyPlayerCatalog : PlayerCatalogBase
    {
        private readonly ImmutableList<Assembly> assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyPlayerCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for players.</param>
        public AssemblyPlayerCatalog(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyPlayerCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for players.</param>
        public AssemblyPlayerCatalog(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = (assemblies ?? throw new ArgumentNullException(nameof(assemblies))).ToImmutableList();
        }

        /// <inheritdoc/>
        protected override IEnumerable<ICatalogPlayer> GetPlayers(Type gameStateType, Type moveType)
        {
            var playerUnconstructed = typeof(IPlayer<,>);
            var playerInterface = playerUnconstructed.MakeGenericType(gameStateType, moveType);
            var playerInterfaceInfo = playerInterface.GetTypeInfo();

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
                    if (playerInterfaceInfo.IsAssignableFrom(typeInfo))
                    {
                        yield return new CatalogPlayer(type, gameStateType, moveType, ReflectionUtilities.GetPublicInitializers(type));
                        continue;
                    }

                    if (!type.IsGenericTypeDefinition || typeInfo.IsAbstract)
                    {
                        continue;
                    }

                    var targetDefinitions = typeInfo
                        .ImplementedInterfaces
                        .Where(i => i == playerUnconstructed || (i.IsConstructedGenericType && i.GetGenericTypeDefinition() == playerUnconstructed));
                    if (targetDefinitions.Any())
                    {
                        foreach (var match in ReflectionUtilities.FixGenericTypeConstraints(typeInfo.GenericTypeParameters, _ => candidateTypeParameters))
                        {
                            var playerType = ReflectionUtilities.TryMakeGenericType(type, match);
                            if (playerType != null)
                            {
                                if (playerInterfaceInfo.IsAssignableFrom(playerType.GetTypeInfo()))
                                {
                                    yield return new CatalogPlayer(playerType, gameStateType, moveType, ReflectionUtilities.GetPublicInitializers(playerType));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
