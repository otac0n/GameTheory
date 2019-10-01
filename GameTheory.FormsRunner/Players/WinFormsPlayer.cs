// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Players
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class WinFormsPlayer<TMove> : IPlayer<TMove>
        where TMove : IMove
    {
        private PlayerView playerView;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsPlayer{TMove}"/> class.
        /// </summary>
        /// <param name="playerToken">The <see cref="PlayerToken"/> that represents the player.</param>
        public WinFormsPlayer(PlayerToken playerToken)
        {
            this.PlayerToken = playerToken;
            this.playerView = new PlayerView(playerToken, typeof(TMove));
            this.playerView.MessageSent += this.MessageSent;
            this.playerView.Show();
        }

        /// <inheritdoc/>
        public event EventHandler<MessageSentEventArgs> MessageSent;

        /// <inheritdoc/>
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc/>
        public Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel) => this.playerView.ChooseMove(state, cancel);

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!this.playerView.IsDisposed)
            {
                this.playerView.InvokeIfRequired(() => this.playerView.Dispose());
            }
        }
    }
}
