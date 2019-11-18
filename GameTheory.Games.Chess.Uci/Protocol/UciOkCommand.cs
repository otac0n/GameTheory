// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "uciok" command.
    /// </summary>
    public class UciOkCommand : Command
    {
        private UciOkCommand()
            : base("uciok")
        {
        }

        /// <summary>
        /// Gets an instance of the <see cref="UciOkCommand"/>.
        /// </summary>
        public static UciOkCommand Instance { get; } = new UciOkCommand();
    }
}
