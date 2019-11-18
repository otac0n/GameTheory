// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// And unrecognized command.
    /// </summary>
    public class UnknownCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownCommand"/> class.
        /// </summary>
        /// <param name="text">The text of the command.</param>
        public UnknownCommand(string text)
            : base(null)
        {
            this.Text = text;
        }

        /// <summary>
        /// Gets the text of the command.
        /// </summary>
        public string Text { get; }

        /// <inheritdoc/>
        public override string ToString() => this.Text;
    }
}
