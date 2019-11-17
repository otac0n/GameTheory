// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class OptionCommand : Command
    {
        public OptionCommand(string name, string type, string @default, string min, string max, IEnumerable<string> vars)
            : base("option")
        {
            this.Name = name;
            this.Type = type;
            this.Default = @default;
            this.Min = min;
            this.Max = max;
            this.Vars = (vars ?? Array.Empty<string>()).ToImmutableList();
        }

        public string Default { get; }

        public string Max { get; }

        public string Min { get; }

        public string Name { get; }

        public string Type { get; }

        public ImmutableList<string> Vars { get; }

        public override string ToString() =>
            $"option name {this.Name} type {this.Type}" +
            (string.IsNullOrEmpty(this.Default) ? string.Empty : $" default {this.Default}") +
            (string.IsNullOrEmpty(this.Min) ? string.Empty : $" min {this.Min}") +
            (string.IsNullOrEmpty(this.Max) ? string.Empty : $" max {this.Max}") +
            string.Concat(this.Vars.Select(v => $" var {v}"));
    }
}
