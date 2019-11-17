// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class UnknownCommand : Command
    {
        public UnknownCommand(string text)
            : base(null)
        {
            this.Text = text;
        }

        public string Text { get; }

        public override string ToString() => this.Text;
    }
}
