// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a default console renderer for types that override the <see cref="object.ToString"/> method.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that will be displayed.</typeparam>
    /// <typeparam name="TMove">The type of moves in the game state.</typeparam>
    public class ToStringConsoleRenderer<TGameState, TMove> : ConsoleRendererBase<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <inheritdoc/>
        public override void Show(TGameState state, PlayerToken playerToken = null) => Console.WriteLine(state);
    }
}
