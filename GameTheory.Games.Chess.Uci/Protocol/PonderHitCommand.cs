// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class PonderHitCommand : Command
    {
        private PonderHitCommand()
            : base("ponderhit")
        {
        }

        public static PonderHitCommand Instance { get; } = new PonderHitCommand();
    }
}
