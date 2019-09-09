// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
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
                StartGame(this.newGameForm.SelectedGame, this.newGameForm.StartingState, this.newGameForm.SelectedPlayers);
                this.newGameForm.Dispose();
            }
        }

        private static void StartGame(IGame selectedGame, object startingState, object[] selectedPlayers)
        {
            var game = (Task)typeof(GameManagerForm).GetMethod(nameof(PlayGame), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(selectedGame.MoveType).Invoke(null, new object[] { startingState, selectedPlayers });
        }

        private static Task PlayGame<TMove>(IGameState<TMove> state, object[] players)
            where TMove : IMove
        {
            var playersList = players.Select(p => (IPlayer<TMove>)p).ToArray();
            return GameUtilities.PlayGame(state, playerTokens => playersList, (a, b, c) =>
            {
            });
        }

        private void QuitMenu_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
