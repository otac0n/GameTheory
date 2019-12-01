// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Players
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Catalogs;

    public class WinFormsPlayer<TGameState, TMove> : IPlayer<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        private PlayerView playerView;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsPlayer{TGameState, TMove}"/> class.
        /// </summary>
        /// <param name="playerToken">The <see cref="PlayerToken"/> that represents the player.</param>
        /// <param name="game">The <see cref="ICatalogGame"/> that describes the game being played.</param>
        public WinFormsPlayer(PlayerToken playerToken, ICatalogGame game)
        {
            this.PlayerToken = playerToken;
            this.playerView = new PlayerView(playerToken, game);
            this.playerView.MessageSent += this.MessageSent;
            this.playerView.Show();
        }

        /// <inheritdoc/>
        public event EventHandler<MessageSentEventArgs> MessageSent;

        /// <inheritdoc/>
        public PlayerToken PlayerToken { get; }

        /// <inheritdoc/>
        public Task<Maybe<TMove>> ChooseMove(TGameState state, CancellationToken cancel) => this.playerView.ChooseMove<TGameState, TMove>(state, cancel);

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
