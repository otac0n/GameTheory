// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class PositionCommand : Command
    {
        public PositionCommand(string position, IEnumerable<string> moves)
            : base("position")
        {
            this.Position = position;
            this.Moves = (moves ?? Array.Empty<string>()).ToImmutableList();
        }

        public ImmutableList<string> Moves { get; }

        public string Position { get; }

        public override string ToString() =>
            this.Verb +
            (this.Position == null ? string.Empty : this.Position == GameState.StartingPosition ? " startpos" : " fen " + this.Position) +
            (this.Moves.IsEmpty ? string.Empty : " moves" + string.Concat(this.Moves.Select(m => " " + m)));
    }
}
