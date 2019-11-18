// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "isready" command.
    /// </summary>
    public class IsReadyCommand : Command
    {
        private IsReadyCommand()
            : base("isready")
        {
        }

        /// <summary>
        /// Gets an instance of the <see cref="IsReadyCommand"/>.
        /// </summary>
        public static IsReadyCommand Instance { get; } = new IsReadyCommand();
    }
}
