// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class IsReadyCommand : Command
    {
        private IsReadyCommand()
            : base("isready")
        {
        }

        public static IsReadyCommand Instance { get; } = new IsReadyCommand();
    }
}
