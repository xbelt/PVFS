namespace VFS_GUI
{
    partial class AdvancedSearch
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
            this.SearchTermBox = new System.Windows.Forms.TextBox();
            this.SearchAdvancedButton = new System.Windows.Forms.Button();
            this.caseSensitiveBox = new System.Windows.Forms.CheckBox();
            this.metricDistanceBox = new System.Windows.Forms.CheckBox();
            this.onlyFilesBox = new System.Windows.Forms.CheckBox();
            this.onlyFoldersBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // SearchTermBox
            // 
            this.SearchTermBox.Location = new System.Drawing.Point(12, 82);
            this.SearchTermBox.Name = "SearchTermBox";
            this.SearchTermBox.Size = new System.Drawing.Size(240, 20);
            this.SearchTermBox.TabIndex = 1;
            this.SearchTermBox.Text = "Enter Search Term";
            this.SearchTermBox.Click += new System.EventHandler(this.SearchTermBox_Click);
            this.SearchTermBox.TextChanged += new System.EventHandler(this.SearchTermBox_TextChanged);
            // 
            // SearchAdvancedButton
            // 
            this.SearchAdvancedButton.Location = new System.Drawing.Point(12, 108);
            this.SearchAdvancedButton.Name = "SearchAdvancedButton";
            this.SearchAdvancedButton.Size = new System.Drawing.Size(240, 23);
            this.SearchAdvancedButton.TabIndex = 2;
            this.SearchAdvancedButton.Text = "Search";
            this.SearchAdvancedButton.UseVisualStyleBackColor = true;
            this.SearchAdvancedButton.Click += new System.EventHandler(this.AdvancedSearchButton_Click);
            // 
            // caseSensitiveBox
            // 
            this.caseSensitiveBox.AutoSize = true;
            this.caseSensitiveBox.Location = new System.Drawing.Point(13, 13);
            this.caseSensitiveBox.Name = "caseSensitiveBox";
            this.caseSensitiveBox.Size = new System.Drawing.Size(125, 17);
            this.caseSensitiveBox.TabIndex = 3;
            this.caseSensitiveBox.Text = "Apply Case Sensitive";
            this.caseSensitiveBox.UseVisualStyleBackColor = true;
            this.caseSensitiveBox.CheckedChanged += new System.EventHandler(this.caseSensitiveBox_CheckedChanged);
            // 
            // metricDistanceBox
            // 
            this.metricDistanceBox.AutoSize = true;
            this.metricDistanceBox.Location = new System.Drawing.Point(144, 13);
            this.metricDistanceBox.Name = "metricDistanceBox";
            this.metricDistanceBox.Size = new System.Drawing.Size(100, 17);
            this.metricDistanceBox.TabIndex = 4;
            this.metricDistanceBox.Text = "Metric Distance";
            this.metricDistanceBox.UseVisualStyleBackColor = true;
            this.metricDistanceBox.CheckedChanged += new System.EventHandler(this.metricDistanceBox_CheckedChanged);
            // 
            // onlyFilesBox
            // 
            this.onlyFilesBox.AutoSize = true;
            this.onlyFilesBox.Location = new System.Drawing.Point(13, 49);
            this.onlyFilesBox.Name = "onlyFilesBox";
            this.onlyFilesBox.Size = new System.Drawing.Size(71, 17);
            this.onlyFilesBox.TabIndex = 5;
            this.onlyFilesBox.Text = "Only Files";
            this.onlyFilesBox.UseVisualStyleBackColor = true;
            this.onlyFilesBox.CheckedChanged += new System.EventHandler(this.onlyFilesBox_CheckedChanged);
            // 
            // onlyFoldersBox
            // 
            this.onlyFoldersBox.AutoSize = true;
            this.onlyFoldersBox.Location = new System.Drawing.Point(144, 49);
            this.onlyFoldersBox.Name = "onlyFoldersBox";
            this.onlyFoldersBox.Size = new System.Drawing.Size(84, 17);
            this.onlyFoldersBox.TabIndex = 6;
            this.onlyFoldersBox.Text = "Only Folders";
            this.onlyFoldersBox.UseVisualStyleBackColor = true;
            this.onlyFoldersBox.CheckedChanged += new System.EventHandler(this.onlyFoldersBox_CheckedChanged);
            // 
            // AdvancedSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 139);
            this.Controls.Add(this.onlyFoldersBox);
            this.Controls.Add(this.onlyFilesBox);
            this.Controls.Add(this.metricDistanceBox);
            this.Controls.Add(this.caseSensitiveBox);
            this.Controls.Add(this.SearchAdvancedButton);
            this.Controls.Add(this.SearchTermBox);
            this.Name = "AdvancedSearch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Advanced Search";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox SearchTermBox;
        private System.Windows.Forms.Button SearchAdvancedButton;
        private System.Windows.Forms.CheckBox caseSensitiveBox;
        private System.Windows.Forms.CheckBox metricDistanceBox;
        private System.Windows.Forms.CheckBox onlyFilesBox;
        private System.Windows.Forms.CheckBox onlyFoldersBox;
    }
}