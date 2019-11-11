// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class ReadyOkCommand : Command
    {
        private ReadyOkCommand()
            : base("readyok")
        {
        }

        public static ReadyOkCommand Instance { get; } = new ReadyOkCommand();
    }
}
