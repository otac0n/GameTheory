// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The "bestmove" command.
    /// </summary>
    public class BestMoveCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BestMoveCommand"/> class.
        /// </summary>
        /// <param name="move">The chosen move.</param>
        /// <param name="ponder">The move requested for pondering.</param>
        public BestMoveCommand(string move, [Optional] string ponder)
            : base("bestmove")
        {
            if (string.IsNullOrEmpty(move))
            {
                throw new ArgumentNullException(nameof(move));
            }

            this.Move = move;
            this.Ponder = ponder;
        }

        /// <summary>
        /// Gets the chosen move.
        /// </summary>
        public string Move { get; }

        /// <summary>
        /// Gets the move requested for pondering.
        /// </summary>
        public string Ponder { get; }

        /// <inheritdoc/>
        public override string ToString() =>
            string.IsNullOrEmpty(this.Ponder)
                ? $"bestmove {this.Move}"
                : $"bestmove {this.Move} ponder {this.Ponder}";
    }
}
