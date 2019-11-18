// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci
{
    using System;
    using GameTheory.Games.Chess.Uci.Protocol;

    /// <summary>
    /// Contains a message from the player.
    /// </summary>
    public class UnhandledCommandEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledCommandEventArgs"/> class.
        /// </summary>
        /// <param name="command">The unhandled command.</param>
        public UnhandledCommandEventArgs(Command command)
        {
            this.Command = command;
        }

        /// <summary>
        /// Gets the unhandled command.
        /// </summary>
        public Command Command { get; }
    }
}
