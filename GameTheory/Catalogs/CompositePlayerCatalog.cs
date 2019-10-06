// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a composite collection of players as a single collection.
    /// </summary>
    public class CompositePlayerCatalog : PlayerCatalogBase
    {
        private readonly IPlayerCatalog[] catalogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositePlayerCatalog"/> class.
        /// </summary>
        /// <param name="catalogs">The catalogs in this composite collection.</param>
        public CompositePlayerCatalog(params IPlayerCatalog[] catalogs)
        {
            this.catalogs = catalogs.ToArray();
        }

        /// <inheritdoc/>
        protected override IEnumerable<ICatalogPlayer> GetPlayers(Type moveType) => this.catalogs.SelectMany(c => c.FindPlayers(moveType));
    }
}
