// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class InfoCommand : Command
    {
        public InfoCommand(string name, IEnumerable<string> vars)
            : base("info")
        {
            this.Name = name;
            this.Vars = (vars ?? Array.Empty<string>()).ToImmutableList();
        }

        public string Name { get; }

        public ImmutableList<string> Vars { get; }

        public override string ToString() => this.Verb + " " + this.Name + string.Concat(this.Vars.Select(m => " " + m));
    }
}
