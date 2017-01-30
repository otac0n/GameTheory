// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ConsolePlayer<TMove> : IPlayer<TMove>
        where TMove : IMove
    {
        public ConsolePlayer(PlayerToken playerToken)
        {
            this.PlayerToken = playerToken;
        }

        public PlayerToken PlayerToken { get; }

        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> gameState, CancellationToken cancel)
        {
            await Task.Yield();

            var moves = gameState.GetAvailableMoves(this.PlayerToken);
            if (moves.Any())
            {
                Console.WriteLine("Current state:");
                Console.WriteLine(gameState);
                return new Maybe<TMove>(ConsoleInteraction.Choose(moves.ToArray()));
            }
            else
            {
                return default(Maybe<TMove>);
            }
        }

        public void Dispose()
        {
        }
    }
}
