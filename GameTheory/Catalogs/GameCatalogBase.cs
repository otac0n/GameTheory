// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Provides a base implementation for <see cref="IGameCatalog"/>.
    /// </summary>
    public abstract class GameCatalogBase : IGameCatalog
    {
        private readonly Lazy<ImmutableList<ICatalogGame>> availableGames;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCatalogBase"/> class.
        /// </summary>
        public GameCatalogBase()
        {
            this.availableGames = new Lazy<ImmutableList<ICatalogGame>>(() => this.GetGames().ToImmutableList(), isThreadSafe: true);
        }

        /// <inheritdoc/>
        public IList<ICatalogGame> AvailableGames => this.availableGames.Value;

        /// <summary>
        /// Enumerates the games in this catalog.
        /// </summary>
        /// <returns>The enumerable collection of games in the catalog.</returns>
        protected abstract IEnumerable<ICatalogGame> GetGames();
    }
}
