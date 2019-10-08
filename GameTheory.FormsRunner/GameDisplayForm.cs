// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;
    using Unity;
    using static FormsRunner.Shared.Controls;

    public partial class GameDisplayForm : Form
    {
        private readonly IReadOnlyList<Display> displays;

        public GameDisplayForm(IGameInfo gameInfo)
        {
            this.InitializeComponent();
            var container = new UnityContainer();
            container.RegisterInstance(gameInfo.Game);

            this.GameInfo = gameInfo;
            this.GameInfo.Move += this.GameInfo_Move;
            var type = this.GameInfo.Game.GameStateType;
            var displays = new List<Display>
            {
                new PlayerTokenDisplay(this.GameInfo),
            };
            displays.AddRange(Program.DisplayCatalog.FindDisplays(type).Select(d => (Display)container.Resolve(d)));
            this.displays = displays;
            this.RefreshDisplay();
        }

        public IGameInfo GameInfo { get; }

        private void GameInfo_Move(object sender, EventArgs e)
        {
            this.RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            Display.Update(
                this.splitContainer.Panel1.Controls.Cast<Control>().SingleOrDefault(),
                new Scope(),
                this.GameInfo.Game.GameStateType,
                this.GameInfo.GameStates.Last(),
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

            Display.Update(
                this.splitContainer.Panel2.Controls.Cast<Control>().SingleOrDefault(),
                new Scope(),
                this.GameInfo.Moves,
                this.displays,
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
        }

        private class PlayerTokenDisplay : Display
        {
            private IGameInfo gameInfo;

            public PlayerTokenDisplay(IGameInfo gameInfo)
            {
                this.gameInfo = gameInfo;
            }

            public static Color GetPlayerColor(PlayerToken playerToken)
            {
                var bad = SystemColors.Control;
                var bytes = playerToken.Id.ToByteArray();
                var r = (int)bytes[0];
                var g = (int)bytes[1];
                var b = (int)bytes[2];

                int[] v = { r - bad.R, g - bad.G, b - bad.B };
                double[] t =
                {
                    v[0] == 0 ? double.PositiveInfinity : v[0] > 0 ? (255.0 - r) / v[0] : (0.0 - r) / v[0],
                    v[1] == 0 ? double.PositiveInfinity : v[1] > 0 ? (255.0 - g) / v[1] : (0.0 - g) / v[1],
                    v[2] == 0 ? double.PositiveInfinity : v[2] > 0 ? (255.0 - b) / v[2] : (0.0 - b) / v[2],
                };

                var minT = Math.Min(t[0], Math.Min(t[1], t[2]));

                r = (int)Math.Round(r + v[0] * minT);
                g = (int)Math.Round(g + v[1] * minT);
                b = (int)Math.Round(b + v[2] * minT);

                return Color.FromArgb(r, g, b);
            }

            public override bool CanDisplay(Scope scope, Type type, object value) =>
                value is PlayerToken playerToken && this.gameInfo.PlayerTokens.Contains(playerToken);

            protected override Control Update(Control control, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
            {
                var playerToken = (PlayerToken)value;
                var playerName = this.gameInfo.PlayerNames[this.gameInfo.PlayerTokens.IndexOf(playerToken)];

                if (control is Label label && label.Tag == this)
                {
                    label.Text = playerName;
                }
                else
                {
                    label = MakeLabel(playerName, tag: this);
                }

                label.ForeColor = GetPlayerColor(playerToken);
                return label;
            }
        }
    }
}
