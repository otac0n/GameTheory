// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Linq;
    using System.Reflection;
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
            this.gamesList
                .GetType()
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this.gamesList, true, null);
        }

        private static GameInfo<TGameState, TMove> PlayGame<TGameState, TMove>(ICatalogGame game, ICatalogPlayer[] players, TGameState startingState, object[] playerInstances, GameManagerForm parent)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            var gameInfo = new GameInfo<TGameState, TMove>(game, players, startingState, playerInstances.Cast<IPlayer<TGameState, TMove>>().ToArray(), parent);

            return gameInfo;
        }

        private void GameManagerForm_Shown(object sender, EventArgs e)
        {
            this.NewGameMenu_Click(sender, e);
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
                var players = this.newGameForm.SelectedPlayers;
                var startingState = this.newGameForm.StartingState;
                var playerInstances = this.newGameForm.PlayerInstances;
                this.newGameForm.Dispose();
                var gameInfo = this.StartGame(game, players, startingState, playerInstances);
                this.gamesList.Items.Add(gameInfo.Row);
            }
        }

        private void QuitMenu_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private IGameInfo StartGame(ICatalogGame game, ICatalogPlayer[] players, object startingState, object[] playerInstances) =>
            (IGameInfo)typeof(GameManagerForm).GetMethod(nameof(PlayGame), BindingFlags.Static | BindingFlags.NonPublic)
            .MakeGenericMethod(game.GameStateType, game.MoveType)
            .Invoke(null, new object[] { game, players, startingState, playerInstances, this });

        private void ViewGameMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem gameRow in this.gamesList.SelectedItems)
            {
                var gameInfo = gameRow.Tag as IGameInfo;
                gameInfo?.Show();
            }
        }
    }
}
