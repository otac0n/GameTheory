// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a composite collection of displays as a single collection.
    /// </summary>
    public class CompositeDisplayCatalog : DisplayCatalogBase
    {
        private readonly IDisplayCatalog[] catalogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeDisplayCatalog"/> class.
        /// </summary>
        /// <param name="catalogs">The catalogs in this composite collection.</param>
        public CompositeDisplayCatalog(params IDisplayCatalog[] catalogs)
        {
            this.catalogs = catalogs.ToArray();
        }

        /// <inheritdoc/>
        protected override IEnumerable<Type> GetDisplays(Type gameStateType) =>
            this.catalogs.SelectMany(c => c.FindDisplays(gameStateType));
    }
}
