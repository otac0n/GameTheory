// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.Shared.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a composite collection of console renderers as a single collection.
    /// </summary>
    public class CompositeConsoleRendererCatalog : ConsoleRendererCatalogBase
    {
        private readonly IConsoleRendererCatalog[] catalogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeConsoleRendererCatalog"/> class.
        /// </summary>
        /// <param name="catalogs">The catalogs in this composite collection.</param>
        public CompositeConsoleRendererCatalog(params IConsoleRendererCatalog[] catalogs)
        {
            this.catalogs = catalogs.ToArray();
        }

        /// <inheritdoc/>
        protected override IEnumerable<Type> GetConsoleRenderers(Type gameStateType, Type moveType) =>
            this.catalogs.SelectMany(c => c.FindConsoleRenderers(gameStateType, moveType));
    }
}
