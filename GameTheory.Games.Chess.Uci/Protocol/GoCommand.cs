// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "go" command.
    /// </summary>
    public class GoCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoCommand"/> class.
        /// </summary>
        /// <param name="commands">The commands to issue.</param>
        public GoCommand(string commands)
            : base("go")
        {
            this.Commands = commands;
        }

        /// <summary>
        /// Gets a default instance of the <see cref="GoCommand"/>.
        /// </summary>
        public static GoCommand Default { get; } = new GoCommand(null);

        /// <summary>
        /// Gets the commands to issue.
        /// </summary>
        public string Commands { get; }

        /// <inheritdoc/>
        public override string ToString() =>
            string.IsNullOrWhiteSpace(this.Commands)
                ? base.ToString()
                : $"go {this.Commands}";
    }
}
