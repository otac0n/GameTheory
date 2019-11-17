// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class StopCommand : Command
    {
        private StopCommand()
            : base("stop")
        {
        }

        public static StopCommand Instance { get; } = new StopCommand();
    }
}
