// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class RegisterCommand : Command
    {
        public RegisterCommand(string name, string code)
            : base("register")
        {
            this.Name = name;
            this.Code = code;
        }

        public static RegisterCommand Later { get; } = new RegisterCommand(null, null);

        public string Code { get; }

        public string Name { get; }

        public override string ToString() =>
            string.IsNullOrEmpty(this.Name)
                ? string.IsNullOrEmpty(this.Code)
                    ? "register later"
                    : $"register code {this.Code}"
                : string.IsNullOrEmpty(this.Code)
                    ? $"register name {this.Name}"
                    : $"register name {this.Name} code {this.Code}";
    }
}
