// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Provides a base implementation for <see cref="IPlayerCatalog"/>.
    /// </summary>
    public abstract class PlayerCatalogBase : IPlayerCatalog
    {
        private ConcurrentDictionary<Type, IReadOnlyList<ICatalogPlayer>> players = new ConcurrentDictionary<Type, IReadOnlyList<ICatalogPlayer>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerCatalogBase"/> class.
        /// </summary>
        public PlayerCatalogBase()
        {
        }

        /// <inheritdoc/>
        public IReadOnlyList<ICatalogPlayer> FindPlayers(Type moveType) => this.players.GetOrAdd(moveType, _ => this.GetPlayers(moveType).ToImmutableList());

        /// <summary>
        /// Enumerates the players who are capable of playing the specified move type.
        /// </summary>
        /// <param name="moveType">The type of moves to be played.</param>
        /// <returns>The enumerable collection of supported players.</returns>
        protected abstract IEnumerable<ICatalogPlayer> GetPlayers(Type moveType);
    }
}
