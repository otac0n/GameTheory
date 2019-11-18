// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "readyok" command.
    /// </summary>
    public class ReadyOkCommand : Command
    {
        private ReadyOkCommand()
            : base("readyok")
        {
        }

        /// <summary>
        /// Gets an instance of the <see cref="ReadyOkCommand"/>.
        /// </summary>
        public static ReadyOkCommand Instance { get; } = new ReadyOkCommand();
    }
}
