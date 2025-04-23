// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Games.Chess.NotationSystems;
    using GameTheory.Games.Chess.Serialization;
    using GameTheory.Games.Chess.Uci.Protocol;

    public class UciPlayer : IPlayer<GameState, Move>
    {
        private readonly UciEngine engine;
        private readonly ShortCoordinateNotation notationSystem;
        private TaskCompletionSource<BestMoveCommand> moveSource;

        public UciPlayer(PlayerToken playerToken, string fileName, string arguments, IEnumerable<SetOptionCommand> options)
        {
            this.PlayerToken = playerToken;
            this.engine = new UciEngine(fileName, arguments);
            this.engine.UnhandledCommand += this.Engine_UnhandledCommand;
            this.notationSystem = new ShortCoordinateNotation();

            if (options is object)
            {
                foreach (var option in options)
                {
                    this.engine.Execute(option);
                }
            }

            this.engine.Execute(IsReadyCommand.Instance);
        }

        /// <inheritdoc/>
        public event EventHandler<MessageSentEventArgs> MessageSent;

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc/>
        public async Task<Maybe<Move>> ChooseMove(GameState state, CancellationToken cancel)
        {
            if (state.ActivePlayer != this.PlayerToken)
            {
                return default;
            }

            this.moveSource = new TaskCompletionSource<BestMoveCommand>();
            this.engine.Execute(new PositionCommand(Serializer.SerializeFen(state), null));
            this.engine.Execute(GoCommand.Default);

            var bestMove = (await this.moveSource.Task.ConfigureAwait(false)).Move;
            var move = state.GetAvailableMoves().Where(m => string.Concat(this.notationSystem.Format(m)) == bestMove).SingleOrDefault();
            return move != null ? new Maybe<Move>(move) : default;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.engine.Dispose();
            }
        }

        private void Engine_UnhandledCommand(object sender, UnhandledCommandEventArgs e)
        {
            switch (e.Command)
            {
                case BestMoveCommand bestMoveCommand:
                    this.moveSource?.TrySetResult(bestMoveCommand);
                    break;

                case InfoCommand _:
                default:
                    this.RaiseMessageSent(new object[] { e.Command.ToString() });
                    break;
            }
        }

        private void RaiseMessageSent(IList<object> formatTokens)
        {
            this.MessageSent?.Invoke(this, new MessageSentEventArgs(formatTokens));
        }
    }
}
