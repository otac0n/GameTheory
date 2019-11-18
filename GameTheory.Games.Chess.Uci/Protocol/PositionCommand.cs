// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The "position" command.
    /// </summary>
    public class PositionCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PositionCommand"/> class.
        /// </summary>
        /// <param name="position">The position as FEN.</param>
        /// <param name="moves">The moves to continue with from the current or specified position.</param>
        public PositionCommand([Optional] string position, [Optional] IEnumerable<string> moves)
            : base("position")
        {
            this.Position = position;
            this.Moves = (moves ?? Array.Empty<string>()).ToImmutableList();
        }

        /// <summary>
        /// Gets the moves to continue with from the current or specified position.
        /// </summary>
        public ImmutableList<string> Moves { get; }

        /// <summary>
        /// Gets the position as FEN.
        /// </summary>
        public string Position { get; }

        /// <inheritdoc/>
        public override string ToString() =>
            this.Verb +
            (this.Position == null ? string.Empty : this.Position == GameState.StartingPosition ? " startpos" : " fen " + this.Position) +
            (this.Moves.IsEmpty ? string.Empty : " moves" + string.Concat(this.Moves.Select(m => " " + m)));
    }
}
