// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class QuitCommand : Command
    {
        private QuitCommand()
            : base("quit")
        {
        }

        public static QuitCommand Instance { get; } = new QuitCommand();
    }
}
