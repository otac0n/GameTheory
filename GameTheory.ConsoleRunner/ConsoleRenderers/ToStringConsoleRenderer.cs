// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;

    internal class ToStringConsoleRenderer<TMove> : IConsoleRenderer<TMove>
        where TMove : IMove
    {
        public void Show(PlayerToken playerToken, IGameState<TMove> state)
        {
            Console.WriteLine(state);
        }
    }
}
