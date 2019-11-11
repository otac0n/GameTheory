// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class UciNewGameCommand : Command
    {
        private UciNewGameCommand()
            : base("ucinewgame")
        {
        }

        public static UciNewGameCommand Instance { get; } = new UciNewGameCommand();
    }
}
