// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    internal interface IConsoleRenderer<TMove>
        where TMove : IMove
    {
        void Show(PlayerToken playerToken, IGameState<TMove> state);
    }
}