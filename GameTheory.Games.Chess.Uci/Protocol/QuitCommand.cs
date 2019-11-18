// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "quit" command.
    /// </summary>
    public class QuitCommand : Command
    {
        private QuitCommand()
            : base("quit")
        {
        }

        /// <summary>
        /// Gets an instance of the <see cref="QuitCommand"/>.
        /// </summary>
        public static QuitCommand Instance { get; } = new QuitCommand();
    }
}
