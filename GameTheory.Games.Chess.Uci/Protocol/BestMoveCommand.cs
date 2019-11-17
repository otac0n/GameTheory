// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public class BestMoveCommand : Command
    {
        public BestMoveCommand(string move, string ponder)
            : base("bestmove")
        {
            this.Move = move;
            this.Ponder = ponder;
        }

        public string Move { get; }

        public string Ponder { get; }

        public override string ToString() =>
            string.IsNullOrEmpty(this.Ponder)
                ? $"bestmove {this.Move}"
                : $"bestmove {this.Move} ponder {this.Ponder}";
    }
}
