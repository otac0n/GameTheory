// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "debug" command.
    /// </summary>
    public class DebugCommand : Command
    {
        private DebugCommand(string value)
            : base("debug")
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets an instance of the <see cref="DebugCommand"/> with the value "off".
        /// </summary>
        public static DebugCommand Off { get; } = new DebugCommand("off");

        /// <summary>
        /// Gets an instance of the <see cref="DebugCommand"/> with the value "on".
        /// </summary>
        public static DebugCommand On { get; } = new DebugCommand("on");

        /// <summary>
        /// Gets the value of the command.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc/>
        public override string ToString() => $"debug {this.Value}";
    }
}
