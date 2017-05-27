﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
            var playerInterface = typeof(IPlayer<>).MakeGenericType(moveType);
            foreach (var assembly in this.assemblies)
            {
                var staticPlayers = from t in assembly.ExportedTypes
                                    where playerInterface.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())
                                    select t;
                var constructedPlayers = from t in assembly.ExportedTypes
                                         let info = t.GetTypeInfo()
                                         let typeParameters = info.GenericTypeParameters
                                         where typeParameters.Length == 1
                                         let typeParameterInterfaces = typeParameters[0].GetTypeInfo().ImplementedInterfaces.ToList()
                                         where typeParameterInterfaces.Count == 1 && typeParameterInterfaces[0] == typeof(IMove)
                                         let c = t.MakeGenericType(moveType)
                                         where !c.GetTypeInfo().IsAbstract
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