// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Displays;
    using GameTheory.FormsRunner.Shared;
    using Unity;

    public partial class GameDisplayForm : Form
    {
        private readonly IReadOnlyList<Display> displays;
        private readonly Scope scope;

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
                new PlayerTokenDisplay(gameInfo.Game),
            };
            displays.AddRange(Program.DisplayCatalog.FindDisplays(type).Select(d => (Display)container.Resolve(d)));
            this.displays = displays;
            this.scope = new Scope(string.Empty, this);
            this.RefreshDisplay();
        }

        public IGameInfo GameInfo { get; }

        private void GameInfo_Move(object sender, EventArgs e)
        {
            this.RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            var gameState = this.GameInfo.GameStates.Last();
            Display.FindAndUpdate(
                this.splitContainer.Panel1.Controls.Cast<Control>().SingleOrDefault(),
                this.scope.Extend("GameState", gameState),
                this.GameInfo.Game.GameStateType,
                gameState,
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

            Display.FindAndUpdate(
                this.splitContainer.Panel2.Controls.Cast<Control>().SingleOrDefault(),
                this.scope.Extend("Moves", this.GameInfo.Moves),
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
    }
}
