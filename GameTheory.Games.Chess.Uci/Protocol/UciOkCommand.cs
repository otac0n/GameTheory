// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class UciOkCommand : Command
    {
        private UciOkCommand()
            : base("uciok")
        {
        }

        public static UciOkCommand Instance { get; } = new UciOkCommand();
    }
}
