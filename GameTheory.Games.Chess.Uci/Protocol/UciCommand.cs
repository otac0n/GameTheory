// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "uci" command.
    /// </summary>
    public class UciCommand : Command
    {
        private UciCommand()
            : base("uci")
        {
        }

        /// <summary>
        /// Gets an instance of the <see cref="UciCommand"/>.
        /// </summary>
        public static UciCommand Instance { get; } = new UciCommand();
    }
}
