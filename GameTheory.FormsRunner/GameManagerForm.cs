// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using GameTheory.Catalogs;

    /// <summary>
    /// Tracks ongoing games and players.
    /// </summary>
    public partial class GameManagerForm : Form
    {
        private NewGameForm newGameForm;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameManagerForm"/> class.
        /// </summary>
        public GameManagerForm()
        {
            this.InitializeComponent();
        }

        private void NewGameMenu_Click(object sender, System.EventArgs e)
        {
            if (this.newGameForm == null || this.newGameForm.IsDisposed)
            {
                this.newGameForm = new NewGameForm();
            }

            if (this.newGameForm.ShowDialog(this) != DialogResult.Cancel)
            {
                var game = this.newGameForm.SelectedGame;
                var state = this.newGameForm.StartingState;
                var players = this.newGameForm.SelectedPlayers;
                this.newGameForm.Dispose();
                var gameInfo = this.StartGame(game, state, players);
                this.gamesList.Items.Add(gameInfo.Row);
            }
        }

        private static GameInfo<TMove> PlayGame<TMove>(GameManagerForm parent, IGame selectedGame, IGameState<TMove> state, object[] players)
            where TMove : IMove
        {
            var playersList = new IPlayer<TMove>[players.Length];
            for (var i = 0; i < players.Length; i++)
            {
                var player = (IPlayer<TMove>)players[i];
                playersList[i] = player;
            }

            var gameInfo = new GameInfo<TMove>(parent, selectedGame, state, playersList);

            return gameInfo;
        }

        private IGameInfo StartGame(IGame selectedGame, object startingState, object[] selectedPlayers) =>
            (IGameInfo)typeof(GameManagerForm).GetMethod(nameof(PlayGame), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(selectedGame.MoveType).Invoke(null, new object[] { this, selectedGame, startingState, selectedPlayers });

        private void QuitMenu_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private interface IGameInfo
        {
            ListViewItem Row { get; }
        }

        private class GameInfo<TMove> : IGameInfo
            where TMove : IMove
        {
            private GameManagerForm parent;

            public GameInfo(GameManagerForm parent, IGame game, IGameState<TMove> startingState, IPlayer<TMove>[] players)
            {
                this.parent = parent;
                this.Game = game;
                this.StartingState = startingState;
                this.Players = players.ToArray();
                this.Moves = new List<TMove>();
                this.Row = new ListViewItem(new string[4]
                {
                    game.Name,
                    "Pending",
                    string.Join(", ", Enumerable.Range(0, players.Length).Select(i => $"{startingState.GetPlayerName(startingState.Players[i])}: {this.Players[i]}")),
                    string.Empty,
                });

                this.GameTask = Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < this.Players.Length; i++)
                    {
                        var player = this.Players[i];
                        player.MessageSent += (sender, args) =>
                        {
                        };
                    }

                    this.UpdateRow(1, "Playing");

                    var result = GameUtilities.PlayGame(startingState, playerTokens => players, (previousState, move, newState) =>
                    {
                        this.Moves.Add(move);
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

            public IGame Game { get; }

            public IGameState<TMove> StartingState { get; }

            public IPlayer<TMove>[] Players { get; }

            public List<TMove> Moves { get; }
        }
    }
}
