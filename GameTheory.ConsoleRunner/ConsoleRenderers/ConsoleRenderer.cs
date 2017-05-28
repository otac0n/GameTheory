// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    internal static class ConsoleRenderer
    {
        internal static IConsoleRenderer<TMove> Default<TMove>()
            where TMove : IMove
        {
            return new ToStringConsoleRenderer<TMove>();
        }
    }
}
