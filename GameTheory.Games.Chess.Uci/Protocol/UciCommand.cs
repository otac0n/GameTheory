// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class UciCommand : Command
    {
        public static readonly UciCommand Instance = new UciCommand();

        private UciCommand()
            : base("uci")
        {
        }
    }
}
