// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    internal interface IConsoleRenderer<TMove>
        where TMove : IMove
    {
        void Show(IGameState<TMove> state, PlayerToken playerToken = null);
    }
}