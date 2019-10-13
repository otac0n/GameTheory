// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Provides a base implementation for <see cref="IDisplayCatalog"/>.
    /// </summary>
    public abstract class DisplayCatalogBase : IDisplayCatalog
    {
        /// <inheritdoc/>
        public IReadOnlyList<Type> FindDisplays(Type moveType) => this.GetDisplays(moveType).ToImmutableList();

        /// <summary>
        /// Enumerates the displays who are capable of displaying elements in the specified game state type.
        /// </summary>
        /// <param name="gameStateType">The type of game state to be displayed.</param>
        /// <returns>The enumerable collection of supported displays.</returns>
        protected abstract IEnumerable<Type> GetDisplays(Type gameStateType);
    }
}
