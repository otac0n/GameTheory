// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.Shared.Catalogs
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A catalog of available console renderers.
    /// </summary>
    public interface IConsoleRendererCatalog
    {
        /// <summary>
        /// Gets the list of console renderers who are capable of playing the specified move type.
        /// </summary>
        /// <typeparam name="TGameState">The type of game states that will be displayed.</typeparam>
        /// <typeparam name="TMove">The type of moves to be played.</typeparam>
        /// <returns>A collection of supported console renderers.</returns>
        IReadOnlyList<Type> FindConsoleRenderers<TGameState, TMove>()
            where TGameState : IGameState<TMove>
            where TMove : IMove;

        /// <summary>
        /// Gets the list of console renderers who are capable of playing the specified move type.
        /// </summary>
        /// <param name="TGameState">The type of game states that will be displayed.</param>
        /// <param name="moveType">The type of moves to be played.</param>
        /// <returns>A collection of supported console renderers.</returns>
        IReadOnlyList<Type> FindConsoleRenderers(Type gameStateType, Type moveType);
    }
}
