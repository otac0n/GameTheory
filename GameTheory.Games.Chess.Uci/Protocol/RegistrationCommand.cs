// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class RegistrationCommand : Command
    {
        public RegistrationCommand(string status)
            : base("registration")
        {
            this.Status = status;
        }

        public string Status { get; }

        public override string ToString() => $"registration {this.Status}";
    }
}
