// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Provides enumeration for games in an assembly.
    /// </summary>
    public abstract class GameCatalog
    {
        private readonly Lazy<ImmutableList<ICatalogGame>> availableGames;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCatalog"/> class.
        /// </summary>
        public GameCatalog()
        {
            this.availableGames = new Lazy<ImmutableList<ICatalogGame>>(() => this.GetGames().ToImmutableList(), isThreadSafe: true);
        }

        /// <summary>
        /// Gets the available games in the catalog.
        /// </summary>
        public IList<ICatalogGame> AvailableGames => this.availableGames.Value;

        /// <summary>
        /// Enumerates the games in this catalog.
        /// </summary>
        /// <returns>The enumerable collection of games in the catalog.</returns>
        protected abstract IEnumerable<ICatalogGame> GetGames();
    }
}
