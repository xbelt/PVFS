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
            this.AdvancedSearchCheckList = new System.Windows.Forms.CheckedListBox();
            this.SearchTermBox = new System.Windows.Forms.TextBox();
            this.SearchAdvancedButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AdvancedSearchCheckList
            // 
            this.AdvancedSearchCheckList.CheckOnClick = true;
            this.AdvancedSearchCheckList.FormattingEnabled = true;
            this.AdvancedSearchCheckList.Items.AddRange(new object[] {
            "Case Sensitive",
            "Closest Match",
            "Wild Cards (&)",
            "Regular Expression",
            "Restrict to Folders",
            "Restrict to Files"});
            this.AdvancedSearchCheckList.Location = new System.Drawing.Point(12, 12);
            this.AdvancedSearchCheckList.Name = "AdvancedSearchCheckList";
            this.AdvancedSearchCheckList.Size = new System.Drawing.Size(240, 94);
            this.AdvancedSearchCheckList.TabIndex = 0;
            this.AdvancedSearchCheckList.SelectedIndexChanged += new System.EventHandler(this.checkedListBox1_SelectedIndexChanged);
            // 
            // SearchTermBox
            // 
            this.SearchTermBox.Location = new System.Drawing.Point(12, 112);
            this.SearchTermBox.Name = "SearchTermBox";
            this.SearchTermBox.Size = new System.Drawing.Size(240, 20);
            this.SearchTermBox.TabIndex = 1;
            this.SearchTermBox.Text = "Enter Search Term";
            this.SearchTermBox.TextChanged += new System.EventHandler(this.SearchTermBox_TextChanged);
            this.SearchTermBox.Click += new System.EventHandler(this.SearchTermBox_Click);
            // 
            // SearchAdvancedButton
            // 
            this.SearchAdvancedButton.Location = new System.Drawing.Point(12, 138);
            this.SearchAdvancedButton.Name = "SearchAdvancedButton";
            this.SearchAdvancedButton.Size = new System.Drawing.Size(240, 23);
            this.SearchAdvancedButton.TabIndex = 2;
            this.SearchAdvancedButton.Text = "Search";
            this.SearchAdvancedButton.UseVisualStyleBackColor = true;
            this.SearchAdvancedButton.Click += new System.EventHandler(this.AdvancedSearchButton_Click);
            // 
            // AdvancedSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 174);
            this.Controls.Add(this.SearchAdvancedButton);
            this.Controls.Add(this.SearchTermBox);
            this.Controls.Add(this.AdvancedSearchCheckList);
            this.Name = "AdvancedSearch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Advanced Search";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox AdvancedSearchCheckList;
        private System.Windows.Forms.TextBox SearchTermBox;
        private System.Windows.Forms.Button SearchAdvancedButton;
    }
}