// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Media;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using GameTheory.Catalogs;

    public partial class NewGameForm : Form
    {
        private Task<IGame[]> allGamesTask;
        private Task<IGame[]> searchTask;

        /// <summary>
        /// Raised when the user selects a game.
        /// </summary>
        public event EventHandler<GameSelectedEventArgs> GameSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewGameForm"/> class.
        /// </summary>
        public NewGameForm()
        {
            this.InitializeComponent();

            this.allGamesTask = Task.Factory.StartNew(() => Program.Catalog.AvailableGames.OrderBy(k => k.Name, StringComparer.CurrentCultureIgnoreCase).ToArray());
            this.Search(string.Empty);
        }

        private void Search(string searchText)
        {
            var search = searchText.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (search.Length == 0)
            {
                this.searchTask = this.allGamesTask;
            }
            else
            {
                this.searchTask = this.allGamesTask.ContinueWith(results => results.Result.Where(g => search.All(s => g.Name.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) > -1)).OrderBy(g => search.Min(s => g.Name.IndexOf(s, StringComparison.CurrentCultureIgnoreCase))).ToArray());
            }

            this.searchTask.ContinueWith(_ => this.RefreshResults());
            this.RefreshResults();
        }

        private void RefreshResults()
        {
            this.InvokeIfRequired(() =>
            {
                var searchTask = this.searchTask;
                if (searchTask == null || !searchTask.IsCompleted)
                {
                    this.searchResults.Enabled = false;
                    this.searchResults.Items.Clear();
                }
                else
                {
                    this.searchResults.Items.Clear();
                    this.searchResults.Items.AddRange(searchTask.Result.Select(r => new ListViewItem(new[] { r.Name }, 0) { Tag = r }).ToArray());
                    this.searchResults.Enabled = true;
                }
            });
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var selectedItems = this.searchResults.SelectedItems;
            if (selectedItems.Count == 1)
            {
                this.Close();
                this.GameSelected?.Invoke(this, new GameSelectedEventArgs(selectedItems[0].Tag as IGame));
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            this.Search(this.searchBox.Text);
        }

        private void NewGameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        /// <summary>
        /// <see cref="EventArgs"/> for the <see cref="GameSelected"/> event.
        /// </summary>
        public class GameSelectedEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GameSelectedEventArgs"/> class.
            /// </summary>
            /// <param name="game">The game that has been selected.</param>
            public GameSelectedEventArgs(IGame game)
            {
                this.Game = game;
            }

            /// <summary>
            /// Gets the selected game.
            /// </summary>
            public IGame Game { get; }
        }
    }
}
