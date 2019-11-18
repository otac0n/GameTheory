// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "stop" command.
    /// </summary>
    public class StopCommand : Command
    {
        private StopCommand()
            : base("stop")
        {
        }

        /// <summary>
        /// Gets an instance of the <see cref="StopCommand"/>.
        /// </summary>
        public static StopCommand Instance { get; } = new StopCommand();
    }
}
