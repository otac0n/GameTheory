// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class IdCommand : Command
    {
        public IdCommand(string field, string value)
            : base("id")
        {
            this.Field = field;
            this.Value = value;
        }

        public string Field { get; }

        public string Value { get; }
    }
}
