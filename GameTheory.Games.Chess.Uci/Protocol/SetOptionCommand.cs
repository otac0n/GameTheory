// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The "setoption" command.
    /// </summary>
    public class SetOptionCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetOptionCommand"/> class.
        /// </summary>
        /// <param name="name">The name of the option to set.</param>
        /// <param name="value">The new value of the option.</param>
        public SetOptionCommand(string name, [Optional] string value)
            : base("setoption")
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets the name of the option to set.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the new value of the option.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc/>
        public override string ToString() =>
            this.Value == null
                ? $"setoption name {this.Name}"
                : $"setoption name {this.Name} value {this.Value}";
    }
}
