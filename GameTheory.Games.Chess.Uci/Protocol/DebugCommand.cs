// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class DebugCommand : Command
    {
        private DebugCommand(string value)
            : base("debug")
        {
            this.Value = value;
        }

        public static DebugCommand Off { get; } = new DebugCommand("off");

        public static DebugCommand On { get; } = new DebugCommand("on");

        public string Value { get; }

        public override string ToString() => $"debug {this.Value}";
    }
}
