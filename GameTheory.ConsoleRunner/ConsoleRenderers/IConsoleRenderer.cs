// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System.Collections.Generic;

    internal interface IConsoleRenderer<TMove>
        where TMove : IMove
    {
        void Show(IGameState<TMove> state, PlayerToken playerToken = null);

        void Show(IGameState<TMove> state, IList<object> formatTokens);

        void Show(IGameState<TMove> state, ITokenFormattable tokenFormattable);
    }
}
