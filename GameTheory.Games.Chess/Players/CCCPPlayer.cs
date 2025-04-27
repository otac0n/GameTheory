// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Players
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Games.Chess.Moves;

    /// <summary>
    /// Checkmate, Check, Capture, Push player.
    /// </summary>
    /// <remarks>
    /// Inspired by Tom7 (suckerpinch)
    /// https://youtu.be/DpXy041BIlA?t=664
    /// </remarks>
    public class CCCPPlayer : IPlayer<GameState, Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CCCPPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        public CCCPPlayer(PlayerToken playerToken)
        {
            this.PlayerToken = playerToken;
        }

        /// <inheritdoc />
        public event EventHandler<MessageSentEventArgs> MessageSent;

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc />
        public async Task<Maybe<Move>> ChooseMove(GameState state, CancellationToken cancel)
        {
            await Task.Yield();
            var colorDir = (2 * state.Players.IndexOf(this.PlayerToken)) - 1;

            var movesMapped = from move in state.GetAvailableMoves<GameState, Move>(this.PlayerToken)
                              let basicMove = move as BasicMove
                              let isCapture = basicMove != null && state[basicMove.ToIndex] != Pieces.None
                              let push = basicMove != null
                                  ? (state.Variant.GetCoordinates(basicMove.FromIndex).Y - state.Variant.GetCoordinates(basicMove.ToIndex).Y) * colorDir
                                  : 0
                              select new { move, isCapture, push };

            return movesMapped.AllMax((a, b) =>
            {
                var comp = 0;

                if ((comp = a.move.IsCheckmate.CompareTo(b.move.IsCheckmate)) != 0)
                {
                    return comp;
                }

                if ((comp = a.move.IsCheck.CompareTo(b.move.IsCheck)) != 0)
                {
                    return comp;
                }

                if ((comp = a.isCapture.CompareTo(b.isCapture)) != 0)
                {
                    return comp;
                }

                return a.push.CompareTo(b.push);
            }).Select(m => new Maybe<Move>(m.move)).Pick();
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
