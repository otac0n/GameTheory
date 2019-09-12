// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public class GameInfo<TMove> : IGameInfo
        where TMove : IMove
    {
        private GameManagerForm parent;
        private GameDisplayForm displayForm;

        public GameInfo(Catalogs.IGame game, Catalogs.Player[] players, IGameState<TMove> startingState, IPlayer<TMove>[] playerInstances, GameManagerForm parent)
        {
            this.parent = parent;
            this.Game = game;
            this.Players = players.ToArray();
            this.GameStates = new List<IGameState<TMove>> { startingState };
            this.PlayerInstances = playerInstances.ToArray();
            this.Moves = new List<IMove>();

            var columns = new string[4]
            {
                game.Name,
                "Pending",
                string.Join(", ", Enumerable.Range(0, this.Players.Count).Select(i => $"{startingState.GetPlayerName(startingState.Players[i])}: {this.Players[i]}")),
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

                this.UpdateRow(1, "Playing");

                var result = GameUtilities.PlayGame(startingState, playerTokens => this.PlayerInstances, (previousState, move, newState) =>
                {
                    this.Moves.Add(move);
                    this.GameStates.Add(newState);
                    var winners = newState.GetWinners();
                    this.UpdateRow(3, string.Join(", ", winners.Select(w => newState.GetPlayerName(w))));
                }).Result;

                this.UpdateRow(1, "Done");
                return result;
            });
        }

        private void UpdateRow(int index, string value)
        {
            this.parent.InvokeIfRequired(() =>
            {
                this.Row.SubItems[index].Text = value;
                if (index == 0)
                {
                    this.Row.Text = value;
                }
            });
        }

        public Task<IGameState<TMove>> GameTask { get; }

        public ListViewItem Row { get; }

        public Catalogs.IGame Game { get; }

        public List<IGameState<TMove>> GameStates { get; }

        public IReadOnlyList<Catalogs.Player> Players { get; }

        public IReadOnlyList<IPlayer<TMove>> PlayerInstances { get; }

        public List<IMove> Moves { get; }

        IReadOnlyList<object> IGameInfo.PlayerInstances => this.PlayerInstances;

        IReadOnlyList<object> IGameInfo.GameStates => this.GameStates;

        IReadOnlyList<IMove> IGameInfo.Moves => this.Moves;

        public void Show()
        {
            if (this.displayForm == null || this.displayForm.IsDisposed)
            {
                this.displayForm = new GameDisplayForm();
            }

            this.displayForm.Show(this.parent);
        }
    }
}
