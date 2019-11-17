// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class CopyProtectionCommand : Command
    {
        public CopyProtectionCommand(string status)
            : base("copyprotection")
        {
            this.Status = status;
        }

        public string Status { get; }

        public override string ToString() => $"copyprotection {this.Status}";
    }
}
