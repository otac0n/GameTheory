// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using GameTheory.Catalogs;

    public class GameInfo<TGameState, TMove> : IGameInfo
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        private GameDisplayForm displayForm;
        private GameManagerForm parent;

        public GameInfo(ICatalogGame game, ICatalogPlayer[] players, TGameState startingState, IPlayer<TGameState, TMove>[] playerInstances, GameManagerForm parent)
        {
            this.parent = parent;
            this.Game = game;
            this.Players = players.ToArray();
            this.PlayerTokens = startingState.Players.ToArray();
            this.PlayerNames = this.PlayerTokens.Select(p => startingState.GetPlayerName<TGameState, TMove>(p)).ToArray();
            this.GameStates = new List<TGameState> { startingState };
            this.PlayerInstances = playerInstances.ToArray();
            this.Moves = new List<IMove>();

            var columns = new string[4]
            {
                game.Name,
                "Pending",
                string.Join(", ", Enumerable.Range(0, this.Players.Count).Select(i => $"{this.PlayerNames[i]}: {this.Players[i]}")),
                string.Empty,
            };

            this.Row = new ListViewItem(columns)
            {
                Tag = this,
            };

            this.GameTask = Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < this.PlayerInstances.Count; i++)
                {
                    var player = this.PlayerInstances[i];
                    player.MessageSent += (sender, args) =>
                    {
                    };
                }

                this.parent.InvokeIfRequired(() => this.UpdateRow(1, "Playing"));

                var result = GameUtilities.PlayGame(startingState, playerTokens => this.PlayerInstances, (previousState, move, newState) =>
                {
                    this.Moves.Add(move);
                    this.GameStates.Add(newState);
                    var winners = newState.GetWinners();
                    this.parent.InvokeIfRequired(() =>
                    {
                        this.UpdateRow(3, string.Join(", ", winners.Select(w => newState.GetPlayerName<TGameState, TMove>(w))));
                        this.Move?.Invoke(this, new EventArgs());
                    });
                }).Result;

                this.parent.InvokeIfRequired(() => this.UpdateRow(1, "Done"));
                return result;
            });
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Move;

        public ICatalogGame Game { get; }

        public List<TGameState> GameStates { get; }

        IReadOnlyList<object> IGameInfo.GameStates => (IReadOnlyList<object>)this.GameStates;

        public Task<TGameState> GameTask { get; }

        public List<IMove> Moves { get; }

        IReadOnlyList<IMove> IGameInfo.Moves => this.Moves;

        public IReadOnlyList<IPlayer<TGameState, TMove>> PlayerInstances { get; }

        IReadOnlyList<object> IGameInfo.PlayerInstances => this.PlayerInstances;

        public IReadOnlyList<string> PlayerNames { get; }

        public IReadOnlyList<ICatalogPlayer> Players { get; }

        public IReadOnlyList<PlayerToken> PlayerTokens { get; }

        public ListViewItem Row { get; }

        public void Show()
        {
            if (this.displayForm == null || this.displayForm.IsDisposed)
            {
                this.displayForm = new GameDisplayForm(this);
            }

            this.displayForm.Show();
        }

        private void UpdateRow(int index, string value)
        {
            this.Row.SubItems[index].Text = value;
            if (index == 0)
            {
                this.Row.Text = value;
            }
        }
    }
}
