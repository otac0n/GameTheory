// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "ponderhit" command.
    /// </summary>
    public class PonderHitCommand : Command
    {
        private PonderHitCommand()
            : base("ponderhit")
        {
        }

        /// <summary>
        /// Gets an instance of the <see cref="PonderHitCommand"/>.
        /// </summary>
        public static PonderHitCommand Instance { get; } = new PonderHitCommand();
    }
}
