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
            System.Windows.Forms.Label searchLabel;
            System.Windows.Forms.Button okButton;
            System.Windows.Forms.Button cancelButton;
            this.searchBox = new System.Windows.Forms.TextBox();
            this.searchResults = new System.Windows.Forms.ListView();
            searchLabel = new System.Windows.Forms.Label();
            okButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.Location = new System.Drawing.Point(93, 12);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(215, 20);
            this.searchBox.TabIndex = 1;
            this.searchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            // 
            // searchLabel
            // 
            searchLabel.AutoSize = true;
            searchLabel.Location = new System.Drawing.Point(12, 15);
            searchLabel.Name = "searchLabel";
            searchLabel.Size = new System.Drawing.Size(75, 13);
            searchLabel.TabIndex = 0;
            searchLabel.Text = "Game &Search:";
            // 
            // searchResults
            // 
            this.searchResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchResults.Location = new System.Drawing.Point(12, 38);
            this.searchResults.MultiSelect = false;
            this.searchResults.Name = "searchResults";
            this.searchResults.Size = new System.Drawing.Size(296, 271);
            this.searchResults.TabIndex = 2;
            this.searchResults.UseCompatibleStateImageBehavior = false;
            this.searchResults.View = System.Windows.Forms.View.List;
            // 
            // okButton
            // 
            okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            okButton.Location = new System.Drawing.Point(152, 315);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.TabIndex = 3;
            okButton.Text = "&OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // cancelButton
            // 
            cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(233, 315);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.TabIndex = 4;
            cancelButton.Text = "&Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // NewGameForm
            // 
            this.AcceptButton = okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = cancelButton;
            this.ClientSize = new System.Drawing.Size(320, 350);
            this.Controls.Add(cancelButton);
            this.Controls.Add(okButton);
            this.Controls.Add(this.searchResults);
            this.Controls.Add(searchLabel);
            this.Controls.Add(this.searchBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewGameForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Game";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NewGameForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.ListView searchResults;
    }
}