namespace GameTheory.FormsRunner
{
    partial class GameManagerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.MenuStrip topMenu;
            System.Windows.Forms.ToolStripMenuItem newGameMenu;
            System.Windows.Forms.ToolStripMenuItem quitMenu;
            System.Windows.Forms.StatusStrip statusStrip;
            System.Windows.Forms.TabControl managerTabs;
            System.Windows.Forms.TabPage gamesTab;
            System.Windows.Forms.ColumnHeader gameColumn;
            System.Windows.Forms.ColumnHeader stateColumn;
            System.Windows.Forms.ColumnHeader playersColumn;
            System.Windows.Forms.ColumnHeader winnersColumn;
            System.Windows.Forms.ContextMenuStrip gameContextMenu;
            System.Windows.Forms.ToolStripMenuItem viewGameMenuItem;
            this.gameMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.gamesList = new System.Windows.Forms.ListView();
            topMenu = new System.Windows.Forms.MenuStrip();
            newGameMenu = new System.Windows.Forms.ToolStripMenuItem();
            quitMenu = new System.Windows.Forms.ToolStripMenuItem();
            statusStrip = new System.Windows.Forms.StatusStrip();
            managerTabs = new System.Windows.Forms.TabControl();
            gamesTab = new System.Windows.Forms.TabPage();
            gameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            stateColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            playersColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            winnersColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            gameContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            viewGameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            topMenu.SuspendLayout();
            managerTabs.SuspendLayout();
            gamesTab.SuspendLayout();
            gameContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // topMenu
            // 
            topMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameMenu});
            topMenu.Location = new System.Drawing.Point(0, 0);
            topMenu.Name = "topMenu";
            topMenu.Size = new System.Drawing.Size(800, 24);
            topMenu.TabIndex = 0;
            topMenu.Text = "menuStrip1";
            // 
            // gameMenu
            // 
            this.gameMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            newGameMenu,
            quitMenu});
            this.gameMenu.Name = "gameMenu";
            this.gameMenu.Size = new System.Drawing.Size(50, 20);
            this.gameMenu.Text = "Game";
            // 
            // newGameMenu
            // 
            newGameMenu.Name = "newGameMenu";
            newGameMenu.Size = new System.Drawing.Size(132, 22);
            newGameMenu.Text = "New Game";
            newGameMenu.Click += new System.EventHandler(this.NewGameMenu_Click);
            // 
            // quitMenu
            // 
            quitMenu.Name = "quitMenu";
            quitMenu.Size = new System.Drawing.Size(132, 22);
            quitMenu.Text = "Quit";
            quitMenu.Click += new System.EventHandler(this.QuitMenu_Click);
            // 
            // statusStrip
            // 
            statusStrip.Location = new System.Drawing.Point(0, 428);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(800, 22);
            statusStrip.TabIndex = 1;
            statusStrip.Text = "statusStrip1";
            // 
            // managerTabs
            // 
            managerTabs.Controls.Add(gamesTab);
            managerTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            managerTabs.Location = new System.Drawing.Point(0, 24);
            managerTabs.Name = "managerTabs";
            managerTabs.SelectedIndex = 0;
            managerTabs.Size = new System.Drawing.Size(800, 404);
            managerTabs.TabIndex = 2;
            // 
            // gamesTab
            // 
            gamesTab.Controls.Add(this.gamesList);
            gamesTab.Location = new System.Drawing.Point(4, 22);
            gamesTab.Name = "gamesTab";
            gamesTab.Padding = new System.Windows.Forms.Padding(3);
            gamesTab.Size = new System.Drawing.Size(792, 378);
            gamesTab.TabIndex = 0;
            gamesTab.Text = "Games";
            gamesTab.UseVisualStyleBackColor = true;
            // 
            // gamesList
            // 
            this.gamesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            gameColumn,
            stateColumn,
            playersColumn,
            winnersColumn});
            this.gamesList.ContextMenuStrip = gameContextMenu;
            this.gamesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gamesList.FullRowSelect = true;
            this.gamesList.Location = new System.Drawing.Point(3, 3);
            this.gamesList.MultiSelect = false;
            this.gamesList.Name = "gamesList";
            this.gamesList.Size = new System.Drawing.Size(786, 372);
            this.gamesList.TabIndex = 0;
            this.gamesList.UseCompatibleStateImageBehavior = false;
            this.gamesList.View = System.Windows.Forms.View.Details;
            // 
            // gameColumn
            // 
            gameColumn.Text = "Game";
            gameColumn.Width = 200;
            // 
            // stateColumn
            // 
            stateColumn.Text = "State";
            stateColumn.Width = 100;
            // 
            // playersColumn
            // 
            playersColumn.Text = "Players";
            playersColumn.Width = 200;
            // 
            // winnersColumn
            // 
            winnersColumn.Text = "Winners";
            winnersColumn.Width = 100;
            // 
            // gameContextMenu
            // 
            gameContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            viewGameMenuItem});
            gameContextMenu.Name = "gameContextMenu";
            gameContextMenu.Size = new System.Drawing.Size(100, 26);
            // 
            // viewGameMenuItem
            // 
            viewGameMenuItem.Name = "viewGameMenuItem";
            viewGameMenuItem.Size = new System.Drawing.Size(99, 22);
            viewGameMenuItem.Text = "&View";
            viewGameMenuItem.Click += new System.EventHandler(this.ViewGameMenuItem_Click);
            // 
            // GameManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(managerTabs);
            this.Controls.Add(statusStrip);
            this.Controls.Add(topMenu);
            this.MainMenuStrip = topMenu;
            this.Name = "GameManagerForm";
            this.Text = "Game Manager";
            topMenu.ResumeLayout(false);
            topMenu.PerformLayout();
            managerTabs.ResumeLayout(false);
            gamesTab.ResumeLayout(false);
            gameContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStripMenuItem gameMenu;
        private System.Windows.Forms.ListView gamesList;
    }
}

