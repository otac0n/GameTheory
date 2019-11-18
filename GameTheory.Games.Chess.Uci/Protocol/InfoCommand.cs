// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// The "info" command.
    /// </summary>
    public class InfoCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfoCommand"/> class.
        /// </summary>
        /// <param name="name">The name of the information.</param>
        /// <param name="vars">The included information variables.</param>
        public InfoCommand(string name, IEnumerable<string> vars)
            : base("info")
        {
            this.Name = name;
            this.Vars = (vars ?? Array.Empty<string>()).ToImmutableList();
        }

        /// <summary>
        /// Gets the name of the information.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the included information variables.
        /// </summary>
        public ImmutableList<string> Vars { get; }

        /// <inheritdoc/>
        public override string ToString() => this.Verb + " " + this.Name + string.Concat(this.Vars.Select(m => " " + m));
    }
}
