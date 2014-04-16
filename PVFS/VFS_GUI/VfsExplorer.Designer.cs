namespace VFS_GUI
{
    partial class VfsExplorer
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuPanel = new System.Windows.Forms.Panel();
            this.deleteButton = new System.Windows.Forms.Button();
            this.pasteButton = new System.Windows.Forms.Button();
            this.moveButton = new System.Windows.Forms.Button();
            this.copyButton = new System.Windows.Forms.Button();
            this.renameButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.createFileButton = new System.Windows.Forms.Button();
            this.importButton = new System.Windows.Forms.Button();
            this.createDirectoryButton = new System.Windows.Forms.Button();
            this.deleteDiskButton = new System.Windows.Forms.Button();
            this.closeDiskButton = new System.Windows.Forms.Button();
            this.openDiskButton = new System.Windows.Forms.Button();
            this.createDiskButton = new System.Windows.Forms.Button();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.mainListView = new System.Windows.Forms.ListView();
            this.addressPanel = new System.Windows.Forms.Panel();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.mainTreeView = new System.Windows.Forms.TreeView();
            this.statusBarText = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuPanel.SuspendLayout();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.addressPanel.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuPanel
            // 
            this.menuPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menuPanel.Controls.Add(this.deleteButton);
            this.menuPanel.Controls.Add(this.pasteButton);
            this.menuPanel.Controls.Add(this.moveButton);
            this.menuPanel.Controls.Add(this.copyButton);
            this.menuPanel.Controls.Add(this.renameButton);
            this.menuPanel.Controls.Add(this.exportButton);
            this.menuPanel.Controls.Add(this.createFileButton);
            this.menuPanel.Controls.Add(this.importButton);
            this.menuPanel.Controls.Add(this.createDirectoryButton);
            this.menuPanel.Controls.Add(this.deleteDiskButton);
            this.menuPanel.Controls.Add(this.closeDiskButton);
            this.menuPanel.Controls.Add(this.openDiskButton);
            this.menuPanel.Controls.Add(this.createDiskButton);
            this.menuPanel.Location = new System.Drawing.Point(0, 0);
            this.menuPanel.Margin = new System.Windows.Forms.Padding(0);
            this.menuPanel.Name = "menuPanel";
            this.menuPanel.Size = new System.Drawing.Size(711, 52);
            this.menuPanel.TabIndex = 0;
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(660, 6);
            this.deleteButton.Margin = new System.Windows.Forms.Padding(0);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(47, 45);
            this.deleteButton.TabIndex = 11;
            this.deleteButton.Text = "deleteButton";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // pasteButton
            // 
            this.pasteButton.Location = new System.Drawing.Point(607, 6);
            this.pasteButton.Margin = new System.Windows.Forms.Padding(0);
            this.pasteButton.Name = "pasteButton";
            this.pasteButton.Size = new System.Drawing.Size(47, 45);
            this.pasteButton.TabIndex = 11;
            this.pasteButton.Text = "pasteButton";
            this.pasteButton.UseVisualStyleBackColor = true;
            this.pasteButton.Click += new System.EventHandler(this.pasteButton_Click);
            // 
            // moveButton
            // 
            this.moveButton.Location = new System.Drawing.Point(500, 6);
            this.moveButton.Margin = new System.Windows.Forms.Padding(0);
            this.moveButton.Name = "moveButton";
            this.moveButton.Size = new System.Drawing.Size(47, 45);
            this.moveButton.TabIndex = 10;
            this.moveButton.Text = "moveButton";
            this.moveButton.UseVisualStyleBackColor = true;
            this.moveButton.Click += new System.EventHandler(this.moveButton_Click);
            // 
            // copyButton
            // 
            this.copyButton.Location = new System.Drawing.Point(553, 6);
            this.copyButton.Margin = new System.Windows.Forms.Padding(0);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(47, 45);
            this.copyButton.TabIndex = 8;
            this.copyButton.Text = "copyButton";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
            // 
            // renameButton
            // 
            this.renameButton.Location = new System.Drawing.Point(447, 6);
            this.renameButton.Margin = new System.Windows.Forms.Padding(0);
            this.renameButton.Name = "renameButton";
            this.renameButton.Size = new System.Drawing.Size(47, 45);
            this.renameButton.TabIndex = 9;
            this.renameButton.Text = "renameButton";
            this.renameButton.UseVisualStyleBackColor = true;
            this.renameButton.Click += new System.EventHandler(this.renameButton_Click);
            // 
            // exportButton
            // 
            this.exportButton.Location = new System.Drawing.Point(393, 6);
            this.exportButton.Margin = new System.Windows.Forms.Padding(0);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(47, 45);
            this.exportButton.TabIndex = 7;
            this.exportButton.Text = "exportButton";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // createFileButton
            // 
            this.createFileButton.Location = new System.Drawing.Point(287, 6);
            this.createFileButton.Margin = new System.Windows.Forms.Padding(0);
            this.createFileButton.Name = "createFileButton";
            this.createFileButton.Size = new System.Drawing.Size(47, 45);
            this.createFileButton.TabIndex = 6;
            this.createFileButton.Text = "createFileButton";
            this.createFileButton.UseVisualStyleBackColor = true;
            this.createFileButton.Click += new System.EventHandler(this.createFileButton_Click);
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(340, 6);
            this.importButton.Margin = new System.Windows.Forms.Padding(0);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(47, 45);
            this.importButton.TabIndex = 5;
            this.importButton.Text = "importButton";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // createDirectoryButton
            // 
            this.createDirectoryButton.Location = new System.Drawing.Point(233, 6);
            this.createDirectoryButton.Margin = new System.Windows.Forms.Padding(0);
            this.createDirectoryButton.Name = "createDirectoryButton";
            this.createDirectoryButton.Size = new System.Drawing.Size(47, 45);
            this.createDirectoryButton.TabIndex = 4;
            this.createDirectoryButton.Text = "createDirectoryButton";
            this.createDirectoryButton.UseVisualStyleBackColor = true;
            this.createDirectoryButton.Click += new System.EventHandler(this.createDirectoryButton_Click);
            // 
            // deleteDiskButton
            // 
            this.deleteDiskButton.Location = new System.Drawing.Point(167, 6);
            this.deleteDiskButton.Margin = new System.Windows.Forms.Padding(0);
            this.deleteDiskButton.Name = "deleteDiskButton";
            this.deleteDiskButton.Size = new System.Drawing.Size(47, 45);
            this.deleteDiskButton.TabIndex = 3;
            this.deleteDiskButton.Text = "deleteDiskButton";
            this.deleteDiskButton.UseVisualStyleBackColor = true;
            this.deleteDiskButton.Click += new System.EventHandler(this.deleteDiskButton_Click);
            // 
            // closeDiskButton
            // 
            this.closeDiskButton.Location = new System.Drawing.Point(113, 6);
            this.closeDiskButton.Margin = new System.Windows.Forms.Padding(0);
            this.closeDiskButton.Name = "closeDiskButton";
            this.closeDiskButton.Size = new System.Drawing.Size(47, 45);
            this.closeDiskButton.TabIndex = 2;
            this.closeDiskButton.Text = "closeDiskButton";
            this.closeDiskButton.UseVisualStyleBackColor = true;
            this.closeDiskButton.Click += new System.EventHandler(this.closeDiskButton_Click);
            // 
            // openDiskButton
            // 
            this.openDiskButton.Location = new System.Drawing.Point(60, 6);
            this.openDiskButton.Margin = new System.Windows.Forms.Padding(0);
            this.openDiskButton.Name = "openDiskButton";
            this.openDiskButton.Size = new System.Drawing.Size(47, 45);
            this.openDiskButton.TabIndex = 1;
            this.openDiskButton.Text = "openDiskButton";
            this.openDiskButton.UseVisualStyleBackColor = true;
            this.openDiskButton.Click += new System.EventHandler(this.openDiskButton_Click);
            // 
            // createDiskButton
            // 
            this.createDiskButton.Location = new System.Drawing.Point(7, 6);
            this.createDiskButton.Margin = new System.Windows.Forms.Padding(0);
            this.createDiskButton.Name = "createDiskButton";
            this.createDiskButton.Size = new System.Drawing.Size(47, 45);
            this.createDiskButton.TabIndex = 0;
            this.createDiskButton.Text = "createDiskButton";
            this.createDiskButton.UseVisualStyleBackColor = true;
            this.createDiskButton.Click += new System.EventHandler(this.createDiskButton_Click);
            // 
            // mainPanel
            // 
            this.mainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainPanel.Controls.Add(this.mainSplitContainer);
            this.mainPanel.Location = new System.Drawing.Point(0, 58);
            this.mainPanel.Margin = new System.Windows.Forms.Padding(2);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(711, 278);
            this.mainPanel.TabIndex = 1;
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.mainTreeView);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.mainListView);
            this.mainSplitContainer.Panel2.Controls.Add(this.addressPanel);
            this.mainSplitContainer.Size = new System.Drawing.Size(711, 278);
            this.mainSplitContainer.SplitterDistance = 177;
            this.mainSplitContainer.SplitterWidth = 3;
            this.mainSplitContainer.TabIndex = 0;
            // 
            // mainListView
            // 
            this.mainListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainListView.Location = new System.Drawing.Point(2, 26);
            this.mainListView.Margin = new System.Windows.Forms.Padding(2);
            this.mainListView.Name = "mainListView";
            this.mainListView.Size = new System.Drawing.Size(530, 252);
            this.mainListView.TabIndex = 1;
            this.mainListView.UseCompatibleStateImageBehavior = false;
            // 
            // addressPanel
            // 
            this.addressPanel.Controls.Add(this.searchTextBox);
            this.addressPanel.Controls.Add(this.searchButton);
            this.addressPanel.Controls.Add(this.addressTextBox);
            this.addressPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.addressPanel.Location = new System.Drawing.Point(0, 0);
            this.addressPanel.Margin = new System.Windows.Forms.Padding(2);
            this.addressPanel.Name = "addressPanel";
            this.addressPanel.Size = new System.Drawing.Size(531, 26);
            this.addressPanel.TabIndex = 0;
            // 
            // searchTextBox
            // 
            this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchTextBox.Location = new System.Drawing.Point(369, 2);
            this.searchTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(131, 20);
            this.searchTextBox.TabIndex = 1;
            this.searchTextBox.Text = "searchTextBox";
            // 
            // searchButton
            // 
            this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchButton.Location = new System.Drawing.Point(504, 1);
            this.searchButton.Margin = new System.Windows.Forms.Padding(2);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(23, 22);
            this.searchButton.TabIndex = 2;
            this.searchButton.Text = "searchButton";
            this.searchButton.UseVisualStyleBackColor = true;
            // 
            // addressTextBox
            // 
            this.addressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addressTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addressTextBox.Location = new System.Drawing.Point(2, 2);
            this.addressTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(364, 20);
            this.addressTextBox.TabIndex = 0;
            this.addressTextBox.Text = "addressTextBox";
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBarText});
            this.statusBar.Location = new System.Drawing.Point(0, 335);
            this.statusBar.Name = "statusBar";
            this.statusBar.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
            this.statusBar.Size = new System.Drawing.Size(711, 22);
            this.statusBar.TabIndex = 2;
            this.statusBar.Text = "Hello World";
            // 
            // mainTreeView
            // 
            this.mainTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTreeView.Location = new System.Drawing.Point(0, 0);
            this.mainTreeView.Name = "mainTreeView";
            this.mainTreeView.PathSeparator = "/";
            this.mainTreeView.Size = new System.Drawing.Size(177, 278);
            this.mainTreeView.TabIndex = 0;
            // 
            // statusBarText
            // 
            this.statusBarText.Name = "statusBarText";
            this.statusBarText.Size = new System.Drawing.Size(77, 17);
            this.statusBarText.Text = "statusBarText";
            // 
            // VfsExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(711, 357);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.menuPanel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(727, 300);
            this.Name = "VfsExplorer";
            this.Text = "Virtual File System";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VfsExplorer_FormClosing);
            this.menuPanel.ResumeLayout(false);
            this.mainPanel.ResumeLayout(false);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.addressPanel.ResumeLayout(false);
            this.addressPanel.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel menuPanel;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button pasteButton;
        private System.Windows.Forms.Button moveButton;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button renameButton;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button createFileButton;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button createDirectoryButton;
        private System.Windows.Forms.Button deleteDiskButton;
        private System.Windows.Forms.Button closeDiskButton;
        private System.Windows.Forms.Button openDiskButton;
        private System.Windows.Forms.Button createDiskButton;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ListView mainListView;
        private System.Windows.Forms.Panel addressPanel;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.TextBox addressTextBox;
        public System.Windows.Forms.TreeView mainTreeView;
        private System.Windows.Forms.ToolStripStatusLabel statusBarText;
    }
}

