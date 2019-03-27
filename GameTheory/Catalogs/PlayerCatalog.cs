// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides enumeration for players in an assembly.
    /// </summary>
    public sealed class PlayerCatalog
    {
        /// <summary>
        /// Gets the default game catalog.
        /// </summary>
        public static readonly PlayerCatalog Default = new PlayerCatalog(typeof(IGameState<>).GetTypeInfo().Assembly);

        private readonly ImmutableList<Assembly> assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for games</param>
        public PlayerCatalog(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for games</param>
        public PlayerCatalog(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = (assemblies ?? throw new ArgumentNullException(nameof(assemblies))).ToImmutableList();
        }

        /// <summary>
        /// Gets the list of players who are capable of playing the specified move type.
        /// </summary>
        /// <param name="moveType">The type of moves to be played.</param>
        /// <returns>A collection of supported players.</returns>
        public IList<Player> FindPlayers(Type moveType) => this.FindPlayersInternal(moveType).ToImmutableList();

        private IEnumerable<Player> FindPlayersInternal(Type moveType)
        {
            var playerUnconstructed = typeof(IPlayer<>);
            var playerInterface = playerUnconstructed.MakeGenericType(moveType);
            foreach (var assembly in this.assemblies)
            {
                var staticPlayers = from t in assembly.ExportedTypes
                                    where playerInterface.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())
                                    select t;

                var candidateTypeParameters = new List<Type> { moveType };
                if (moveType.IsConstructedGenericType && moveType.GenericTypeArguments.Length == 1)
                {
                    candidateTypeParameters.Add(moveType.GenericTypeArguments.Single());
                }

                var tryMakePlayerType = new Func<Type, Type, Type>((playerType, argument) =>
                {
                    try
                    {
                        return playerType.MakeGenericType(argument);
                    }
                    catch (ArgumentException)
                    {
                        return null;
                    }
                });

                var constructedPlayers = from t in assembly.ExportedTypes
                                         let info = t.GetTypeInfo()
                                         let typeParameters = info.GenericTypeParameters
                                         where typeParameters.Length == 1
                                         where info.ImplementedInterfaces.Any(i => i == playerUnconstructed || (i.IsConstructedGenericType && i.GetGenericTypeDefinition() == playerUnconstructed))
                                         let constraints = typeParameters[0].GetTypeInfo().GetGenericParameterConstraints()
                                         from candidate in candidateTypeParameters
                                         let c = tryMakePlayerType(t, candidate)
                                         where c != null && !c.GetTypeInfo().IsAbstract
                                         where playerInterface.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo())
                                         select c;

                var playerTypes = Enumerable.Concat(
                    staticPlayers,
                    constructedPlayers);
                foreach (var playerType in playerTypes)
                {
                    yield return new Player(playerType, moveType);
                }
            }
        }
    }
}
