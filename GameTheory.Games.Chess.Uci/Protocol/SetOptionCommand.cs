// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class SetOptionCommand : Command
    {
        public SetOptionCommand(string name, string value)
            : base("setoption")
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; }

        public string Value { get; }

        public override string ToString() =>
            this.Value == null
                ? $"setoption name {this.Name}"
                : $"setoption name {this.Name} value {this.Value}";
    }
}
