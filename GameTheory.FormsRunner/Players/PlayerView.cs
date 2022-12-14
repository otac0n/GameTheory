// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Players
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using GameTheory.Catalogs;
    using GameTheory.FormsRunner.Displays;
    using GameTheory.FormsRunner.Shared;
    using Unity;
    using static Shared.Controls;

    public partial class PlayerView : Form
    {
        private readonly IReadOnlyList<Display> displays;
        private readonly Scope scope;

        public PlayerView(PlayerToken playerToken, ICatalogGame game)
        {
            this.InitializeComponent();
            var container = new UnityContainer();
            container.RegisterInstance(game);

            this.Text = playerToken.ToString();
            this.PlayerToken = playerToken;
            var displays = new List<Display>
            {
                new PlayerTokenDisplay(game),
            };
            displays.AddRange(Program.DisplayCatalog.FindDisplays(game.GameStateType).Select(d => (Display)container.Resolve(d)));
            this.displays = displays;
            this.scope = new Scope(string.Empty, this, properties: new Dictionary<string, object>
            {
                [Scope.SharedProperties.PlayerToken] = this.PlayerToken,
            });
        }

        public event EventHandler<MessageSentEventArgs> MessageSent;

        public object GameState { get; set; }

        public PlayerToken PlayerToken { get; }

        internal Task<Maybe<TMove>> ChooseMove<TGameState, TMove>(TGameState state, CancellationToken cancel)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            var tcs = new TaskCompletionSource<Maybe<TMove>>(state);

            cancel.Register(() =>
            {
                tcs.TrySetCanceled();
                this.InvokeIfRequired(() =>
                {
                    if (object.ReferenceEquals(this.GameState, state))
                    {
                        this.splitContainer.Panel2.Controls.DisposeAndClear();
                    }
                });
            });

            this.InvokeIfRequired(() =>
            {
                this.GameState = state;

                Display.FindAndUpdate(
                    this.splitContainer.Panel1.Controls.Cast<Control>().SingleOrDefault(),
                    this.scope.Extend(nameof(this.GameState), state),
                    typeof(TGameState),
                    state,
                    this.displays,
                    (oldControl, newControl) =>
                    {
                        if (oldControl != null)
                        {
                            this.splitContainer.Panel1.Controls.Remove(oldControl);
                            oldControl.Dispose();
                        }

                        if (newControl != null)
                        {
                            this.splitContainer.Panel1.Controls.Add(newControl);
                        }
                    });

                var moves = state.GetAvailableMoves<TGameState, TMove>(this.PlayerToken).Select(m => new MoveChoice(m)).ToList();
                Display.FindAndUpdate(
                    this.splitContainer.Panel2.Controls.Cast<Control>().SingleOrDefault(),
                    this.scope.Extend("Moves", moves),
                    moves,
                    new[] { new ChooseMoveDisplay(m => tcs.TrySetResult(new Maybe<TMove>((TMove)m))) }.Concat(this.displays).ToList(),
                    (oldControl, newControl) =>
                    {
                        if (oldControl != null)
                        {
                            this.splitContainer.Panel2.Controls.Remove(oldControl);
                            oldControl.Dispose();
                        }

                        if (newControl != null)
                        {
                            ((FlowLayoutPanel)newControl).FlowDirection = FlowDirection.TopDown;
                            this.splitContainer.Panel2.Controls.Add(newControl);
                        }
                    });
            });

            return tcs.Task;
        }

        private void GameInfo_Move(object sender, EventArgs e)
        {
            this.RefreshDisplay();
        }

        private void RefreshDisplay()
        {
        }

        private class ChooseMoveDisplay : Display
        {
            private Action<object> chooseMove;

            public ChooseMoveDisplay(Action<object> chooseMove)
            {
                this.chooseMove = chooseMove;
            }

            public override bool CanDisplay(Scope scope, Type type, object value) => type == typeof(MoveChoice);

            protected override Control Update(Control control, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
            {
                var move = ((MoveChoice)value).Move;
                if (control is FlowLayoutPanel flowPanel && flowPanel.Tag == this)
                {
                    flowPanel.SuspendLayout();
                    for (var i = flowPanel.Controls.Count - 1; i >= 2; i--)
                    {
                        var oldControl = flowPanel.Controls[i];
                        flowPanel.Controls.RemoveAt(i);
                        oldControl.Dispose();
                    }
                }
                else
                {
                    flowPanel = MakeFlowPanel(tag: this);
                    flowPanel.SuspendLayout();
                    var button = new Button
                    {
                        Text = "Choose",
                        Tag = this,
                        Anchor = AnchorStyles.Left,
                    };
                    button.Click += (sender, args) => this.chooseMove(move);
                    flowPanel.Controls.Add(button);
                }

                Display.FindAndUpdate(
                    flowPanel.Controls.Count > 1 ? flowPanel.Controls[1] : null,
                    scope,
                    move.GetType(),
                    move,
                    displays,
                    (oldControl, newControl) =>
                    {
                        if (oldControl != null)
                        {
                            flowPanel.Controls.Remove(oldControl);
                            oldControl.Dispose();
                        }

                        if (newControl != null)
                        {
                            newControl.Anchor = AnchorStyles.Left;
                            flowPanel.Controls.Add(newControl);
                        }
                    });

                flowPanel.ResumeLayout();

                return flowPanel;
            }
        }

        private class MoveChoice
        {
            public MoveChoice(object move)
            {
                this.Move = move;
            }

            public object Move { get; }
        }
    }
}
