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
    using GameTheory.FormsRunner.Shared;
    using static Shared.Controls;

    public partial class NewGameForm : Form
    {
        private Task<ICatalogGame[]> allGamesTask;
        private object[] playerInstances;
        private Scope scope;
        private Task<ICatalogGame[]> searchTask;
        private ICatalogPlayer[] selectedPlayers;
        private object startingState;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewGameForm"/> class.
        /// </summary>
        public NewGameForm()
        {
            this.InitializeComponent();
            this.scope = new Scope(string.Empty, this);

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
        public ICatalogGame SelectedGame
        {
            get
            {
                var selectedItems = this.searchResults.SelectedItems;
                return selectedItems.Count == 1 ? selectedItems[0].Tag as ICatalogGame : null;
            }
        }

        /// <summary>
        /// Gets the currently selected players.
        /// </summary>
        public ICatalogPlayer[] SelectedPlayers
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
            var playerInstances = this.PlayerInstances;
            this.PlayerInstancesChanged?.Invoke(this, new PlayerInstancesChangedEventArgs(playerInstances));

            this.finishButton.Enabled = playerInstances != null && playerInstances.All(p => p != null);
        }

        protected virtual void OnSelectedGameChanged()
        {
            this.PlayerInstances = null;
            this.SelectedPlayers = null;
            this.StartingState = null;
            var game = this.SelectedGame;
            this.SelectedGameChanged?.Invoke(this, new SelectedGameChangedEventArgs(game));

            if (game != null)
            {
                Editor.FindAndUpdate(
                    this.configurationTab.Controls.Cast<Control>().SingleOrDefault(),
                    this.scope.Extend(game.Name, this.StartingState),
                    game.GameStateType,
                    this.StartingState,
                    out var errorControl,
                    null,
                    this.errorProvider.SetError,
                    (value, valid) =>
                    {
                        (this.StartingState as IDisposable)?.Dispose();
                        this.StartingState = valid ? value : null;
                    },
                    (oldControl, newControl) =>
                    {
                        if (oldControl != null)
                        {
                            this.configurationTab.Controls.Remove(oldControl);
                            oldControl.Dispose();
                        }

                        if (newControl != null)
                        {
                            this.configurationTab.Controls.Add(newControl);
                        }
                    });
            }
            else
            {
                this.configurationTab.Controls.DisposeAndClear();
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

            this.playersTable.Controls.DisposeAndClear();
            if (state != null)
            {
                var game = this.SelectedGame;
                var countPlayersMethod = typeof(NewGameForm).GetMethod(nameof(CountPlayers), BindingFlags.Static | BindingFlags.NonPublic);
                var countPlayersConstructed = countPlayersMethod.MakeGenericMethod(game.GameStateType, game.MoveType);
                var playerOptions = (PlayerOptions)countPlayersConstructed.Invoke(null, new object[] { state });
                var playerCount = playerOptions.Names.Length;
                this.playersTable.RowCount = playerCount * 2;
                var players = new ICatalogPlayer[playerCount];
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
                        var player = (ICatalogPlayer)playersList.SelectedItem;
                        var previous = this.playersTable.GetControlFromPosition(1, p * 2 + 1);
                        if (previous != null)
                        {
                            this.playersTable.Controls.Remove(previous); // TODO: Dispose.
                        }

                        var editor = Editor.FindAndUpdate(
                            null,
                            this.scope.Extend(player.Name, null),
                            player.PlayerType,
                            null, // TODO: Remember previously selected player?
                            out var errorControl,
                            new Editor[] { new PlayerTokenEditor(playerToken), new CatalogGameEditor(game) },
                            this.errorProvider.SetError,
                            (value, valid) =>
                            {
                                (playerInstances[p] as IDisposable)?.Dispose();
                                playersValid[p] = valid;
                                players[p] = valid ? player : null;
                                playerInstances[p] = valid ? value : null;
                                Touch();
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

        private static PlayerOptions CountPlayers<TGameState, TMove>(TGameState state)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            var players = Program.PlayerCatalog.FindPlayers(typeof(TGameState), typeof(TMove));
            var names = state.Players.Select(p => state.GetPlayerName<TGameState, TMove>(p)).ToArray();
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

        private void FinishButton_Click(object sender, EventArgs e)
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

        private void SearchResults_DoubleClick(object sender, EventArgs e)
        {
            this.NextButton_Click(sender, e);
        }

        private void SearchResults_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.OnSelectedGameChanged();
        }

        private void WizardTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ix = this.wizardTabs.SelectedIndex;
            this.backButton.Enabled = ix != 0;
            this.nextButton.Enabled = ix != this.wizardTabs.TabCount - 1;
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
            public SelectedGameChangedEventArgs(ICatalogGame game)
            {
                this.Game = game;
            }

            /// <summary>
            /// Gets the selected game.
            /// </summary>
            public ICatalogGame Game { get; }
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
            public SelectedPlayersChangedEventArgs(ICatalogPlayer[] players)
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

        private class CatalogGameEditor : Editor
        {
            private ICatalogGame game;

            public CatalogGameEditor(ICatalogGame game)
            {
                this.game = game;
            }

            public override bool CanEdit(Scope scope, Type type, object value) => type == typeof(ICatalogGame);

            protected override Control Update(Control control, Scope scope, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set)
            {
                if (control is Label label && label.Tag == this)
                {
                    set(this.game, true);
                }
                else
                {
                    label = MakeLabel(this.game.Name, tag: this);
                    label.AddMargin(bottom: 7);
                    set(this.game, true);
                }

                errorControl = label;
                return label;
            }
        }

        private class PlayerOptions
        {
            public PlayerOptions(string[] names, PlayerToken[] playerTokens, IReadOnlyList<ICatalogPlayer> players)
            {
                this.Names = names;
                this.Players = players;
                this.PlayerTokens = playerTokens;
            }

            public string[] Names { get; }

            public IReadOnlyList<ICatalogPlayer> Players { get; }

            public PlayerToken[] PlayerTokens { get; }
        }

        private class PlayerTokenEditor : Editor
        {
            private PlayerToken playerToken;

            public PlayerTokenEditor(PlayerToken playerToken)
            {
                this.playerToken = playerToken;
            }

            public override bool CanEdit(Scope scope, Type type, object value) => type == typeof(PlayerToken);

            protected override Control Update(Control control, Scope scope, Type type, object value, out Control errorControl, IReadOnlyList<Editor> editors, Action<Control, string> setError, Action<object, bool> set)
            {
                if (control is Label label && label.Tag == this)
                {
                    set(this.playerToken, true);
                }
                else
                {
                    label = MakeLabel(scope.Name, tag: this);
                    label.AddMargin(bottom: 7);
                    set(this.playerToken, true);
                }

                errorControl = label;
                return label;
            }
        }
    }
}
