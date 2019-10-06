// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a composite collection of games as a single collection.
    /// </summary>
    public class CompositeGameCatalog : GameCatalog
    {
        private readonly GameCatalog[] catalogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeGameCatalog"/> class.
        /// </summary>
        /// <param name="catalogs">The catalogs in this composite collection.</param>
        public CompositeGameCatalog(params GameCatalog[] catalogs)
        {
            this.catalogs = catalogs.ToArray();
        }

        /// <inheritdoc/>
        protected override IEnumerable<ICatalogGame> GetGames() => this.catalogs.SelectMany(c => c.AvailableGames);
    }
}
