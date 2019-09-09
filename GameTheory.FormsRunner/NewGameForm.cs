// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Media;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using GameTheory.Catalogs;

    public partial class NewGameForm : Form
    {
        private Task<IGame[]> allGamesTask;
        private Task<IGame[]> searchTask;
        private object startingState;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewGameForm"/> class.
        /// </summary>
        public NewGameForm()
        {
            this.InitializeComponent();

            this.allGamesTask = Task.Factory.StartNew(() => Program.GameCatalog.AvailableGames.OrderBy(k => k.Name, StringComparer.CurrentCultureIgnoreCase).ToArray());
            this.Search(string.Empty);
        }

        /// <summary>
        /// Raised when the user changes the game selection.
        /// </summary>
        public event EventHandler<SelectedGameChangedEventArgs> SelectedGameChanged;

        /// <summary>
        /// Raised when the user changes the starting state.
        /// </summary>
        public event EventHandler<StartingStateChangedEventArgs> StartingStateChanged;

        /// <summary>
        /// Gets the currently selected game.
        /// </summary>
        public IGame SelectedGame
        {
            get
            {
                var selectedItems = this.searchResults.SelectedItems;
                return selectedItems.Count == 1 ? selectedItems[0].Tag as IGame : null;
            }
        }

        /// <summary>
        /// Gets the currently selected starting state.
        /// </summary>
        public object StartingState
        {
            get { return this.startingState; }

            private set
            {
                if (!object.ReferenceEquals(this.startingState, value))
                {
                    this.startingState = value;
                    this.OnStartingStateChanged();
                }
            }
        }

        protected virtual void OnSelectedGameChanged()
        {
            this.StartingState = null;
            var game = this.SelectedGame;
            this.SelectedGameChanged?.Invoke(this, new SelectedGameChangedEventArgs(game));

            this.configurationTab.Controls.Clear(); // TODO: Dispose controls.
            if (game != null)
            {
                var editor = ObjectGraphEditor.MakeEditor(game.Name, game.GameStateType, out var errorControl, out var label, this.errorProvider.SetError, (value, valid) =>
                {
                    this.StartingState = valid ? value : null;
                });

                if (label != null)
                {
                    var propertiesTable = new TableLayoutPanel
                    {
                        AutoSize = true,
                        ColumnCount = 2,
                        RowCount = 1,
                    };
                    propertiesTable.Controls.Add(label, 0, 0);
                    propertiesTable.Controls.Add(editor, 1, 0);

                    // TODO: Dispose controls when propertiesTable is disposed.
                    this.configurationTab.Controls.Add(propertiesTable);
                }
                else
                {
                    this.configurationTab.Controls.Add(editor);
                }
            }
        }

        protected virtual void OnStartingStateChanged()
        {
            var state = this.StartingState;
            this.StartingStateChanged?.Invoke(this, new StartingStateChangedEventArgs(state));

            if (state != null)
            {
                var game = this.SelectedGame;
                typeof(NewGameForm).GetMethod(nameof(CountPlayers), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(game.MoveType).Invoke(null, new object[] { state });
            }
        }

        private static void CountPlayers<TMove>(IGameState<TMove> state)
            where TMove : IMove
        {
            var players = Program.PlayerCatalog.FindPlayers(typeof(TMove));
            var names = state.Players.Select(state.GetPlayerName).ToArray();
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

        private void Finish()
        {
            if (true)
            {
                this.Close();
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void NewGameForm_Shown(object sender, EventArgs e)
        {
            this.wizardTabs.SelectedIndex = 0;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            var ix = this.wizardTabs.SelectedIndex;
            if (ix > 0)
            {
                this.wizardTabs.SelectedIndex = ix - 1;
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            var ix = this.wizardTabs.SelectedIndex;
            if (ix < this.wizardTabs.TabCount - 1)
            {
                this.wizardTabs.SelectedIndex = ix + 1;
            }
            else
            {
                this.Finish();
            }
        }

        private void WizardTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ix = this.wizardTabs.SelectedIndex;

            // TODO: set "Finish" button text.
            this.backButton.Enabled = ix != 0;
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                this.searchResults.Focus();
            }
        }

        private void SearchBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.IsInputKey = true;
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

        private void SearchResults_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.OnSelectedGameChanged();
        }

        /// <summary>
        /// <see cref="EventArgs"/> for the <see cref="SelectedGameChanged"/> event.
        /// </summary>
        public class SelectedGameChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectedGameChangedEventArgs"/> class.
            /// </summary>
            /// <param name="game">The game that has been selected.</param>
            public SelectedGameChangedEventArgs(IGame game)
            {
                this.Game = game;
            }

            /// <summary>
            /// Gets the selected game.
            /// </summary>
            public IGame Game { get; }
        }

        /// <summary>
        /// <see cref="EventArgs"/> for the <see cref="StartingStateChanged"/> event.
        /// </summary>
        public class StartingStateChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StartingStateChangedEventArgs"/> class.
            /// </summary>
            /// <param name="startingState">The starting game state.</param>
            public StartingStateChangedEventArgs(object startingState)
            {
                this.StartingState = startingState;
            }

            /// <summary>
            /// Gets the starting game state.
            /// </summary>
            public object StartingState { get; }
        }
    }
}
