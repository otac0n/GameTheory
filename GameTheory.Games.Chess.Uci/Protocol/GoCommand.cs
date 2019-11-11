// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class GoCommand : Command
    {
        public GoCommand(string commands)
            : base("go")
        {
            this.Commands = commands;
        }

        public static GoCommand Default { get; } = new GoCommand(null);

        public string Commands { get; }

        public override string ToString() =>
            string.IsNullOrWhiteSpace(this.Commands)
                ? base.ToString()
                : $"go {this.Commands}";
    }
}
