// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Games.Chess.NotationSystems;
    using GameTheory.Games.Chess.Uci.Protocol;

    public class UciPlayer : IPlayer<GameState, Move>
    {
        private UciEngine engine;
        private TaskCompletionSource<BestMoveCommand> moveSource;
        private ShortCoordinateNotation notationSystem;

        public UciPlayer(PlayerToken playerToken, string fileName, string arguments = null)
        {
            this.PlayerToken = playerToken;
            this.engine = new UciEngine(fileName, arguments);
            this.engine.UnhandledCommand += this.Engine_UnhandledCommand;
            this.engine.Execute(IsReadyCommand.Instance);
            this.notationSystem = new ShortCoordinateNotation();
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
            this.engine.Dispose();
        }

        private void Engine_UnhandledCommand(object sender, UnhandledCommandEventArgs e)
        {
            switch (e.Command)
            {
                case BestMoveCommand bestMoveCommand:
                    this.moveSource?.TrySetResult(bestMoveCommand);
                    break;

                case InfoCommand infoCommand:
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
