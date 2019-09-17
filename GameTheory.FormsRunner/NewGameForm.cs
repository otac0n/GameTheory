// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
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
        private object[] playerInstances;
        private Task<IGame[]> searchTask;
        private Player[] selectedPlayers;
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
        /// Raised when the user changes the player instances.
        /// </summary>
        public event EventHandler<PlayerInstancesChangedEventArgs> PlayerInstancesChanged;

        /// <summary>
        /// Raised when the user changes the game selection.
        /// </summary>
        public event EventHandler<SelectedGameChangedEventArgs> SelectedGameChanged;

        /// <summary>
        /// Raised when the user changes the players.
        /// </summary>
        public event EventHandler<SelectedPlayersChangedEventArgs> SelectedPlayersChanged;

        /// <summary>
        /// Raised when the user changes the starting state.
        /// </summary>
        public event EventHandler<StartingStateChangedEventArgs> StartingStateChanged;

        /// <summary>
        /// Gets the currently selected player instances.
        /// </summary>
        public object[] PlayerInstances
        {
            get
            {
                return this.playerInstances;
            }

            private set
            {
                if (this.playerInstances != null || value != null)
                {
                    this.playerInstances = value;
                    this.OnPlayerInstancesChanged();
                }
            }
        }

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
        /// Gets the currently selected players.
        /// </summary>
        public Player[] SelectedPlayers
        {
            get
            {
                return this.selectedPlayers;
            }

            private set
            {
                if (this.selectedPlayers != null || value != null)
                {
                    this.selectedPlayers = value;
                    this.OnSelectedPlayersChanged();
                }
            }
        }

        /// <summary>
        /// Gets the currently selected starting state.
        /// </summary>
        public object StartingState
        {
            get
            {
                return this.startingState;
            }

            private set
            {
                if (!object.ReferenceEquals(this.startingState, value))
                {
                    this.startingState = value;
                    this.OnStartingStateChanged();
                }
            }
        }

        protected virtual void OnPlayerInstancesChanged()
        {
            this.PlayerInstancesChanged?.Invoke(this, new PlayerInstancesChangedEventArgs(this.PlayerInstances));
        }

        protected virtual void OnSelectedGameChanged()
        {
            this.PlayerInstances = null;
            this.SelectedPlayers = null;
            this.StartingState = null;
            var game = this.SelectedGame;
            this.SelectedGameChanged?.Invoke(this, new SelectedGameChangedEventArgs(game));

            this.configurationTab.Controls.Clear(); // TODO: Dispose controls.
            if (game != null)
            {
                var editor = ObjectGraphEditor.MakeEditor(null, game.Name, game.GameStateType, null, out var errorControl, out var label, this.errorProvider.SetError, (value, valid) =>
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

        protected virtual void OnSelectedPlayersChanged()
        {
            this.SelectedPlayersChanged?.Invoke(this, new SelectedPlayersChangedEventArgs(this.SelectedPlayers));
        }

        protected virtual void OnStartingStateChanged()
        {
            this.PlayerInstances = null;
            this.SelectedPlayers = null;
            var state = this.StartingState;
            this.StartingStateChanged?.Invoke(this, new StartingStateChangedEventArgs(state));

            this.playersTable.Controls.Clear(); // TODO: Dispose controls.
            if (state != null)
            {
                var game = this.SelectedGame;
                var playerOptions = (PlayerOptions)typeof(NewGameForm).GetMethod(nameof(CountPlayers), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(game.MoveType).Invoke(null, new object[] { state });
                var playerCount = playerOptions.Names.Length;
                this.playersTable.RowCount = playerCount * 2;
                var players = new Player[playerCount];
                var playerInstances = new object[playerCount];
                var playersValid = new bool[playerCount];

                void Touch()
                {
                    var allValid = playersValid.All(p => p);
                    if (!allValid)
                    {
                        this.PlayerInstances = null;
                        this.SelectedPlayers = null;
                    }
                    else
                    {
                        this.playerInstances = null;
                        this.SelectedPlayers = players;
                        this.PlayerInstances = playerInstances;
                    }
                }

                for (var i = 0; i < playerCount; i++)
                {
                    var p = i; // Closure variable.
                    var name = playerOptions.Names[p];
                    var playerToken = playerOptions.PlayerTokens[p];
                    var label = new Label
                    {
                        AutoSize = true,
                        Text = name,
                    };
                    var playersList = new ComboBox
                    {
                        DisplayMember = "Name",
                        DropDownStyle = ComboBoxStyle.DropDownList,
                    };
                    label.Margin = new Padding(label.Margin.Left, label.Margin.Top + 5, label.Margin.Right, label.Margin.Bottom);
                    playersList.Margin = new Padding(playersList.Margin.Left, playersList.Margin.Top, playersList.Margin.Right + 32, playersList.Margin.Bottom);

                    playersList.SelectedValueChanged += (_, a) =>
                    {
                        var player = (Player)playersList.SelectedItem;
                        var previous = this.playersTable.GetControlFromPosition(1, p * 2 + 1);
                        if (previous != null)
                        {
                            this.playersTable.Controls.Remove(previous); // TODO: Dispose.
                        }

                        var editor = ObjectGraphEditor.MakeEditor(
                            null,
                            player.Name,
                            player.PlayerType,
                            null, // TODO: Remember previously selected player?
                            out var errorControl,
                            out var playerLabel,
                            this.errorProvider.SetError,
                            (value, valid) =>
                            {
                                playersValid[p] = valid;
                                players[p] = valid ? player : null;
                                playerInstances[p] = valid ? value : null;
                                Touch();
                            },
                            (string innerPath, string innerName, Type type, object innerValue, out Control innerControl, out Control innerErrorControl, out Label innerLabel, Action<Control, string> setError, Action<object, bool> set) =>
                            {
                                if (type == typeof(PlayerToken))
                                {
                                    innerControl = new Label
                                    {
                                        AutoSize = true,
                                        Text = name,
                                    };
                                    innerControl.Margin = new Padding(innerControl.Margin.Left, innerControl.Margin.Top, innerControl.Margin.Right, innerControl.Margin.Bottom + 7);
                                    innerErrorControl = innerControl;
                                    innerLabel = new Label
                                    {
                                        AutoSize = true,
                                        Text = innerName,
                                    };
                                    set(playerToken, true);
                                    return true;
                                }
                                else
                                {
                                    innerControl = null;
                                    innerErrorControl = null;
                                    innerLabel = null;
                                    return false;
                                }
                            });

                        this.playersTable.Controls.Add(editor, 1, p * 2 + 1);
                    };

                    playersList.Items.AddRange(playerOptions.Players.ToArray());
                    if (playersList.Items.Count > 0)
                    {
                        playersList.SelectedIndex = 0;
                    }

                    this.playersTable.Controls.Add(label, 0, p * 2);
                    this.playersTable.Controls.Add(playersList, 1, p * 2);
                }
            }
            else
            {
                this.playersTable.RowCount = 0;
            }
        }

        private static PlayerOptions CountPlayers<TMove>(IGameState<TMove> state)
            where TMove : IMove
        {
            var players = Program.PlayerCatalog.FindPlayers(typeof(TMove));
            var names = state.Players.Select(state.GetPlayerName).ToArray();
            return new PlayerOptions(names, state.Players.ToArray(), players);
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            var ix = this.wizardTabs.SelectedIndex;
            if (ix > 0)
            {
                this.wizardTabs.SelectedIndex = ix - 1;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void SearchResults_DoubleClick(object sender, EventArgs e)
        {
            this.NextButton_Click(sender, e);
        }

        private void Finish()
        {
            if (true)
            {
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void NewGameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.CancelButton_Click(sender, e);
            }
        }

        private void NewGameForm_Shown(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;
            this.wizardTabs.SelectedIndex = 0;
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

            if (this.searchTask.IsCompleted)
            {
                this.RefreshResults();
            }
            else
            {
                this.RefreshResults();
                this.searchTask.ContinueWith(_ => this.RefreshResults());
            }
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

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            this.Search(this.searchBox.Text);
        }

        private void SearchResults_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.OnSelectedGameChanged();
        }

        private void WizardTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ix = this.wizardTabs.SelectedIndex;

            // TODO: set "Finish" button text.
            this.backButton.Enabled = ix != 0;
        }

        /// <summary>
        /// <see cref="EventArgs"/> for the <see cref="PlayerInstancesChanged"/> event.
        /// </summary>
        public class PlayerInstancesChangedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PlayerInstancesChangedEventArgs"/> class.
            /// </summary>
            /// <param name="playersInstances">The player instances chosen.</param>
            public PlayerInstancesChangedEventArgs(object[] playersInstances)
            {
                this.PlayerInstances = playersInstances;
            }

            /// <summary>
            /// Gets the player instances chosen.
            /// </summary>
            public object[] PlayerInstances { get; }
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
        /// <see cref="EventArgs"/> for the <see cref="SelectedPlayersChanged"/> event.
        /// </summary>
        public class SelectedPlayersChangedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectedPlayersChangedEventArgs"/> class.
            /// </summary>
            /// <param name="players">The players chosen.</param>
            public SelectedPlayersChangedEventArgs(Player[] players)
            {
                this.Players = players;
            }

            /// <summary>
            /// Gets the players chosen.
            /// </summary>
            public object[] Players { get; }
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

        private class PlayerOptions
        {
            public PlayerOptions(string[] names, PlayerToken[] playerTokens, IList<Player> players)
            {
                this.Names = names;
                this.Players = players;
                this.PlayerTokens = playerTokens;
            }

            public string[] Names { get; }

            public IList<Player> Players { get; }

            public PlayerToken[] PlayerTokens { get; }
        }
    }
}
