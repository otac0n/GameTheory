// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.Shared.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Provides a base implementation for <see cref="IConsoleRendererCatalog"/>.
    /// </summary>
    public abstract class ConsoleRendererCatalogBase : IConsoleRendererCatalog
    {
        /// <inheritdoc/>
        public IReadOnlyList<Type> FindConsoleRenderers<TGameState, TMove>()
            where TGameState : IGameState<TMove>
            where TMove : IMove =>
            this.FindConsoleRenderers(typeof(TGameState), typeof(TMove));

        /// <inheritdoc/>
        public IReadOnlyList<Type> FindConsoleRenderers(Type gameStateType, Type moveType) => this.GetConsoleRenderers(gameStateType, moveType).ToImmutableList();

        /// <summary>
        /// Enumerates the console renderers in this catalog.
        /// </summary>
        /// <returns>The enumerable collection of console renderers in the catalog.</returns>
        protected abstract IEnumerable<Type> GetConsoleRenderers(Type gameStateType, Type moveType);
    }
}
