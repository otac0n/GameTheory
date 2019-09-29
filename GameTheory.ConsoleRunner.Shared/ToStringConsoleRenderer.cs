// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.Shared
{
    using System;

    /// <summary>
    /// Provides a default console renderer for types that override the <see cref="object.ToString"/> method.
    /// </summary>
    /// <typeparam name="TMove">The type of moves in the game state.</typeparam>
    public class ToStringConsoleRenderer<TMove> : BaseConsoleRenderer<TMove>
        where TMove : IMove
    {
        /// <inheritdoc/>
        public override void Show(IGameState<TMove> state, PlayerToken playerToken) => Console.WriteLine(state);
    }
}