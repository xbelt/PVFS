namespace VFS_Network
{
    partial class VfsServer
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
            System.Windows.Forms.ColumnHeader columnHeader1;
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Andrea", 0);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Fabian", 0);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Lukas", 1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VfsServer));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.localIPLabel = new System.Windows.Forms.Label();
            this.publicIPLabel = new System.Windows.Forms.Label();
            this.portNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.startServerButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.deleteUserButton = new System.Windows.Forms.Button();
            this.addUserButton = new System.Windows.Forms.Button();
            this.userListView = new System.Windows.Forms.ListView();
            this.smallImageList = new System.Windows.Forms.ImageList(this.components);
            this.logTextBox = new System.Windows.Forms.TextBox();
            columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "User";
            columnHeader1.Width = 200;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Local IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Public IP:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Port:";
            // 
            // localIPLabel
            // 
            this.localIPLabel.AutoSize = true;
            this.localIPLabel.Location = new System.Drawing.Point(117, 16);
            this.localIPLabel.Name = "localIPLabel";
            this.localIPLabel.Size = new System.Drawing.Size(70, 13);
            this.localIPLabel.TabIndex = 1;
            this.localIPLabel.Text = "192.168.1.27";
            // 
            // publicIPLabel
            // 
            this.publicIPLabel.AutoSize = true;
            this.publicIPLabel.Location = new System.Drawing.Point(117, 38);
            this.publicIPLabel.Name = "publicIPLabel";
            this.publicIPLabel.Size = new System.Drawing.Size(16, 13);
            this.publicIPLabel.TabIndex = 3;
            this.publicIPLabel.Text = "...";
            // 
            // portNumericUpDown
            // 
            this.portNumericUpDown.Location = new System.Drawing.Point(120, 58);
            this.portNumericUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.portNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.portNumericUpDown.Name = "portNumericUpDown";
            this.portNumericUpDown.Size = new System.Drawing.Size(93, 20);
            this.portNumericUpDown.TabIndex = 5;
            this.portNumericUpDown.Value = new decimal(new int[] {
            11102,
            0,
            0,
            0});
            // 
            // startServerButton
            // 
            this.startServerButton.Location = new System.Drawing.Point(6, 84);
            this.startServerButton.Name = "startServerButton";
            this.startServerButton.Size = new System.Drawing.Size(207, 34);
            this.startServerButton.TabIndex = 6;
            this.startServerButton.Text = "Start";
            this.startServerButton.UseVisualStyleBackColor = true;
            this.startServerButton.Click += new System.EventHandler(this.startServerButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.startServerButton);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.portNumericUpDown);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.publicIPLabel);
            this.groupBox1.Controls.Add(this.localIPLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(219, 124);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.deleteUserButton);
            this.groupBox2.Controls.Add(this.addUserButton);
            this.groupBox2.Controls.Add(this.userListView);
            this.groupBox2.Location = new System.Drawing.Point(12, 142);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(219, 184);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Users";
            // 
            // deleteUserButton
            // 
            this.deleteUserButton.Location = new System.Drawing.Point(120, 155);
            this.deleteUserButton.Name = "deleteUserButton";
            this.deleteUserButton.Size = new System.Drawing.Size(93, 23);
            this.deleteUserButton.TabIndex = 2;
            this.deleteUserButton.Text = "Delete";
            this.deleteUserButton.UseVisualStyleBackColor = true;
            this.deleteUserButton.Click += new System.EventHandler(this.deleteUserButton_Click);
            // 
            // addUserButton
            // 
            this.addUserButton.Location = new System.Drawing.Point(6, 155);
            this.addUserButton.Name = "addUserButton";
            this.addUserButton.Size = new System.Drawing.Size(95, 23);
            this.addUserButton.TabIndex = 1;
            this.addUserButton.Text = "Add";
            this.addUserButton.UseVisualStyleBackColor = true;
            this.addUserButton.Click += new System.EventHandler(this.addUserButton_Click);
            // 
            // userListView
            // 
            this.userListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
            this.userListView.FullRowSelect = true;
            this.userListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.userListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.userListView.Location = new System.Drawing.Point(6, 19);
            this.userListView.MultiSelect = false;
            this.userListView.Name = "userListView";
            this.userListView.Size = new System.Drawing.Size(207, 130);
            this.userListView.SmallImageList = this.smallImageList;
            this.userListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.userListView.TabIndex = 0;
            this.userListView.UseCompatibleStateImageBehavior = false;
            this.userListView.View = System.Windows.Forms.View.Details;
            // 
            // smallImageList
            // 
            this.smallImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("smallImageList.ImageStream")));
            this.smallImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.smallImageList.Images.SetKeyName(0, "offline.png");
            this.smallImageList.Images.SetKeyName(1, "online.png");
            // 
            // logTextBox
            // 
            this.logTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logTextBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.Location = new System.Drawing.Point(237, 12);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(314, 314);
            this.logTextBox.TabIndex = 2;
            // 
            // VfsServer
            // 
            this.AcceptButton = this.startServerButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 338);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MinimumSize = new System.Drawing.Size(406, 376);
            this.Name = "VfsServer";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Virtual File System: Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VfsServer_FormClosing);
            this.Load += new System.EventHandler(this.VfsServer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label localIPLabel;
        private System.Windows.Forms.Label publicIPLabel;
        private System.Windows.Forms.NumericUpDown portNumericUpDown;
        private System.Windows.Forms.Button startServerButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button deleteUserButton;
        private System.Windows.Forms.Button addUserButton;
        private System.Windows.Forms.ListView userListView;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.ImageList smallImageList;
    }
}