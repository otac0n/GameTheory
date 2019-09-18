namespace GameTheory.FormsRunner
{
    partial class NewGameForm
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
            System.Windows.Forms.Button cancelButton;
            System.Windows.Forms.TabPage gameTab;
            System.Windows.Forms.Label searchLabel;
            System.Windows.Forms.TabPage playersTab;
            this.nextButton = new System.Windows.Forms.Button();
            this.searchResults = new System.Windows.Forms.ListView();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.playersTable = new System.Windows.Forms.TableLayoutPanel();
            this.configurationTab = new System.Windows.Forms.TabPage();
            this.wizardTabs = new System.Windows.Forms.TabControl();
            this.backButton = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.finishButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            gameTab = new System.Windows.Forms.TabPage();
            searchLabel = new System.Windows.Forms.Label();
            playersTab = new System.Windows.Forms.TabPage();
            gameTab.SuspendLayout();
            playersTab.SuspendLayout();
            this.wizardTabs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // nextButton
            // 
            this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.nextButton.Location = new System.Drawing.Point(203, 378);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 2;
            this.nextButton.Text = "&Next >";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // cancelButton
            // 
            cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(374, 378);
            cancelButton.Margin = new System.Windows.Forms.Padding(12, 3, 3, 3);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.TabIndex = 4;
            cancelButton.Text = "&Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // gameTab
            // 
            gameTab.Controls.Add(this.searchResults);
            gameTab.Controls.Add(searchLabel);
            gameTab.Controls.Add(this.searchBox);
            gameTab.Location = new System.Drawing.Point(4, 22);
            gameTab.Name = "gameTab";
            gameTab.Padding = new System.Windows.Forms.Padding(3);
            gameTab.Size = new System.Drawing.Size(429, 334);
            gameTab.TabIndex = 0;
            gameTab.Text = "Game";
            gameTab.UseVisualStyleBackColor = true;
            // 
            // searchResults
            // 
            this.searchResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchResults.Location = new System.Drawing.Point(6, 32);
            this.searchResults.MultiSelect = false;
            this.searchResults.Name = "searchResults";
            this.searchResults.Size = new System.Drawing.Size(417, 296);
            this.searchResults.TabIndex = 2;
            this.searchResults.UseCompatibleStateImageBehavior = false;
            this.searchResults.View = System.Windows.Forms.View.List;
            this.searchResults.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.SearchResults_ItemSelectionChanged);
            this.searchResults.DoubleClick += new System.EventHandler(this.SearchResults_DoubleClick);
            // 
            // searchLabel
            // 
            searchLabel.AutoSize = true;
            searchLabel.Location = new System.Drawing.Point(6, 9);
            searchLabel.Name = "searchLabel";
            searchLabel.Size = new System.Drawing.Size(44, 13);
            searchLabel.TabIndex = 0;
            searchLabel.Text = "&Search:";
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.Location = new System.Drawing.Point(56, 6);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(367, 20);
            this.searchBox.TabIndex = 1;
            this.searchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchBox_KeyDown);
            this.searchBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.SearchBox_PreviewKeyDown);
            // 
            // playersTab
            // 
            playersTab.Controls.Add(this.playersTable);
            playersTab.Location = new System.Drawing.Point(4, 22);
            playersTab.Name = "playersTab";
            playersTab.Padding = new System.Windows.Forms.Padding(3);
            playersTab.Size = new System.Drawing.Size(429, 334);
            playersTab.TabIndex = 2;
            playersTab.Text = "Players";
            playersTab.UseVisualStyleBackColor = true;
            // 
            // playersTable
            // 
            this.playersTable.ColumnCount = 2;
            this.playersTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.playersTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.playersTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.playersTable.Location = new System.Drawing.Point(3, 3);
            this.playersTable.Name = "playersTable";
            this.playersTable.RowCount = 1;
            this.playersTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.playersTable.Size = new System.Drawing.Size(423, 328);
            this.playersTable.TabIndex = 0;
            // 
            // configurationTab
            // 
            this.configurationTab.Location = new System.Drawing.Point(4, 22);
            this.configurationTab.Name = "configurationTab";
            this.configurationTab.Padding = new System.Windows.Forms.Padding(3);
            this.configurationTab.Size = new System.Drawing.Size(429, 334);
            this.configurationTab.TabIndex = 1;
            this.configurationTab.Text = "Configure";
            this.configurationTab.UseVisualStyleBackColor = true;
            // 
            // wizardTabs
            // 
            this.wizardTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wizardTabs.Controls.Add(gameTab);
            this.wizardTabs.Controls.Add(this.configurationTab);
            this.wizardTabs.Controls.Add(playersTab);
            this.wizardTabs.Location = new System.Drawing.Point(12, 12);
            this.wizardTabs.Name = "wizardTabs";
            this.wizardTabs.SelectedIndex = 0;
            this.wizardTabs.Size = new System.Drawing.Size(437, 360);
            this.wizardTabs.TabIndex = 0;
            this.wizardTabs.SelectedIndexChanged += new System.EventHandler(this.WizardTabs_SelectedIndexChanged);
            // 
            // backButton
            // 
            this.backButton.Enabled = false;
            this.backButton.Location = new System.Drawing.Point(122, 378);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(75, 23);
            this.backButton.TabIndex = 1;
            this.backButton.Text = "< &Back";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // finishButton
            // 
            this.finishButton.Enabled = false;
            this.finishButton.Location = new System.Drawing.Point(284, 378);
            this.finishButton.Name = "finishButton";
            this.finishButton.Size = new System.Drawing.Size(75, 23);
            this.finishButton.TabIndex = 3;
            this.finishButton.Text = "&Finish";
            this.finishButton.UseVisualStyleBackColor = true;
            this.finishButton.Click += new System.EventHandler(this.FinishButton_Click);
            // 
            // NewGameForm
            // 
            this.AcceptButton = this.finishButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = cancelButton;
            this.ClientSize = new System.Drawing.Size(461, 413);
            this.Controls.Add(this.finishButton);
            this.Controls.Add(this.backButton);
            this.Controls.Add(this.wizardTabs);
            this.Controls.Add(cancelButton);
            this.Controls.Add(this.nextButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewGameForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Game";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NewGameForm_FormClosing);
            this.Shown += new System.EventHandler(this.NewGameForm_Shown);
            gameTab.ResumeLayout(false);
            gameTab.PerformLayout();
            playersTab.ResumeLayout(false);
            this.wizardTabs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl wizardTabs;
        private System.Windows.Forms.ListView searchResults;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.TabPage configurationTab;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.TableLayoutPanel playersTable;
        private System.Windows.Forms.Button finishButton;
        private System.Windows.Forms.Button nextButton;
    }
}
