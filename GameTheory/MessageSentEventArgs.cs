// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Contains a message from the player.
    /// </summary>
    public class MessageSentEventArgs : EventArgs, ITokenFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSentEventArgs"/> class.
        /// </summary>
        /// <param name="formatTokens">The format tokens in the message.</param>
        public MessageSentEventArgs(params object[] formatTokens)
            : this((IList<object>)formatTokens)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSentEventArgs"/> class.
        /// </summary>
        /// <param name="formatTokens">The format tokens in the message.</param>
        public MessageSentEventArgs(IList<object> formatTokens)
        {
            this.FormatTokens = formatTokens;
        }

        /// <summary>
        /// Gets the format tokens in the message.
        /// </summary>
        public IList<object> FormatTokens { get; }
    }
}
