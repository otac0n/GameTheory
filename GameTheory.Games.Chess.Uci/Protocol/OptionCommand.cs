// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The "option" command.
    /// </summary>
    public class OptionCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionCommand"/> class.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="type">The type of the option.</param>
        /// <param name="default">The default value.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="vars">A list of predefined values.</param>
        public OptionCommand(string name, string type, [Optional] string @default, [Optional] string min, [Optional] string max, [Optional] IEnumerable<string> vars)
            : base("option")
        {
            this.Name = name;
            this.Type = type;
            this.Default = @default;
            this.Min = min;
            this.Max = max;
            this.Vars = (vars ?? Array.Empty<string>()).ToImmutableList();
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public string Default { get; }

        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        public string Max { get; }

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        public string Min { get; }

        /// <summary>
        /// Gets the name of the option.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the option.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets a list of predefined values.
        /// </summary>
        public ImmutableList<string> Vars { get; }

        /// <inheritdoc/>
        public override string ToString() =>
            $"option name {this.Name} type {this.Type}" +
            (this.Default == null ? string.Empty : $" default {this.Default}") +
            (string.IsNullOrEmpty(this.Min) ? string.Empty : $" min {this.Min}") +
            (string.IsNullOrEmpty(this.Max) ? string.Empty : $" max {this.Max}") +
            string.Concat(this.Vars.Select(v => $" var {v}"));
    }
}
