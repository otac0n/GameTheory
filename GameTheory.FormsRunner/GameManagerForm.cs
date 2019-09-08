// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System.Windows.Forms;

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
                this.newGameForm.GameSelected += (_, args) =>
                {
                    MessageBox.Show($"Selected {args.Game.Name} ({args.Game.GameStateType.FullName})");
                };
            }

            this.newGameForm.Show(this);
        }

        private void QuitMenu_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
