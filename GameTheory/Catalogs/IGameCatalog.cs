// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System.Collections.Generic;

    /// <summary>
    /// A catalog of available games.
    /// </summary>
    public interface IGameCatalog
    {
        /// <summary>
        /// Gets the available games in the catalog.
        /// </summary>
        IList<ICatalogGame> AvailableGames { get; }
    }
}
