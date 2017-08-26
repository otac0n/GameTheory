// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;

    internal class ToStringConsoleRenderer<TMove> : IConsoleRenderer<TMove>
        where TMove : IMove
    {
        public void Show(IGameState<TMove> state, PlayerToken playerToken)
        {
            Console.WriteLine(state);
        }
    }
}
