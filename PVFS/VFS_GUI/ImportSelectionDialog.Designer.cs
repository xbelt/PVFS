namespace VFS_GUI
{
    partial class ImportSelectionDialog
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
            this.FileImportButton = new System.Windows.Forms.Button();
            this.FolderImportButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FileImportButton
            // 
            this.FileImportButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileImportButton.Location = new System.Drawing.Point(12, 12);
            this.FileImportButton.Name = "FileImportButton";
            this.FileImportButton.Size = new System.Drawing.Size(231, 42);
            this.FileImportButton.TabIndex = 0;
            this.FileImportButton.Text = "File";
            this.FileImportButton.UseVisualStyleBackColor = true;
            this.FileImportButton.Click += new System.EventHandler(this.FileButton_Click);
            // 
            // FolderImportButton
            // 
            this.FolderImportButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FolderImportButton.Location = new System.Drawing.Point(12, 60);
            this.FolderImportButton.Name = "FolderImportButton";
            this.FolderImportButton.Size = new System.Drawing.Size(231, 45);
            this.FolderImportButton.TabIndex = 0;
            this.FolderImportButton.Text = "Folder";
            this.FolderImportButton.UseVisualStyleBackColor = true;
            this.FolderImportButton.Click += new System.EventHandler(this.FolderButton_Click);
            // 
            // ImportSelectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(255, 121);
            this.Controls.Add(this.FolderImportButton);
            this.Controls.Add(this.FileImportButton);
            this.Name = "ImportSelectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button FileImportButton;
        private System.Windows.Forms.Button FolderImportButton;
    }
}