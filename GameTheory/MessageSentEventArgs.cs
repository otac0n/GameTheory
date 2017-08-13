// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;

    /// <summary>
    /// Contains a message from the player.
    /// </summary>
    public class MessageSentEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSentEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MessageSentEventArgs(string message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; }
    }
}
