namespace VFS_Network
{
    partial class VfsClient
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
            this.ipLabel = new System.Windows.Forms.Label();
            this.portLabel = new System.Windows.Forms.Label();
            this.userLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.ipTextBox = new System.Windows.Forms.TextBox();
            this.portNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.userTextBox = new System.Windows.Forms.TextBox();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.loginButton = new System.Windows.Forms.Button();
            this.logoutButton = new System.Windows.Forms.Button();
            this.explorerButton = new System.Windows.Forms.Button();
            this.resetTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // ipLabel
            // 
            this.ipLabel.AutoSize = true;
            this.ipLabel.Location = new System.Drawing.Point(12, 15);
            this.ipLabel.Name = "ipLabel";
            this.ipLabel.Size = new System.Drawing.Size(54, 13);
            this.ipLabel.TabIndex = 0;
            this.ipLabel.Text = "Server IP:";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(12, 40);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(63, 13);
            this.portLabel.TabIndex = 2;
            this.portLabel.Text = "Server Port:";
            // 
            // userLabel
            // 
            this.userLabel.AutoSize = true;
            this.userLabel.Location = new System.Drawing.Point(12, 67);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(63, 13);
            this.userLabel.TabIndex = 4;
            this.userLabel.Text = "User Name:";
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(12, 93);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(56, 13);
            this.passwordLabel.TabIndex = 6;
            this.passwordLabel.Text = "Password:";
            // 
            // ipTextBox
            // 
            this.ipTextBox.Location = new System.Drawing.Point(121, 12);
            this.ipTextBox.Name = "ipTextBox";
            this.ipTextBox.Size = new System.Drawing.Size(100, 20);
            this.ipTextBox.TabIndex = 1;
            // 
            // portNumericUpDown
            // 
            this.portNumericUpDown.Location = new System.Drawing.Point(121, 38);
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
            this.portNumericUpDown.Size = new System.Drawing.Size(100, 20);
            this.portNumericUpDown.TabIndex = 3;
            this.portNumericUpDown.Value = new decimal(new int[] {
            11102,
            0,
            0,
            0});
            // 
            // userTextBox
            // 
            this.userTextBox.Location = new System.Drawing.Point(121, 64);
            this.userTextBox.Name = "userTextBox";
            this.userTextBox.Size = new System.Drawing.Size(100, 20);
            this.userTextBox.TabIndex = 5;
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(121, 90);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(100, 20);
            this.passwordTextBox.TabIndex = 7;
            // 
            // loginButton
            // 
            this.loginButton.Location = new System.Drawing.Point(12, 116);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(209, 34);
            this.loginButton.TabIndex = 8;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // logoutButton
            // 
            this.logoutButton.Enabled = false;
            this.logoutButton.Location = new System.Drawing.Point(12, 156);
            this.logoutButton.Name = "logoutButton";
            this.logoutButton.Size = new System.Drawing.Size(209, 34);
            this.logoutButton.TabIndex = 9;
            this.logoutButton.Text = "Logout";
            this.logoutButton.UseVisualStyleBackColor = true;
            this.logoutButton.Click += new System.EventHandler(this.logoutButton_Click);
            // 
            // explorerButton
            // 
            this.explorerButton.Enabled = false;
            this.explorerButton.Location = new System.Drawing.Point(12, 196);
            this.explorerButton.Name = "explorerButton";
            this.explorerButton.Size = new System.Drawing.Size(209, 34);
            this.explorerButton.TabIndex = 10;
            this.explorerButton.Text = "Open Explorer";
            this.explorerButton.UseVisualStyleBackColor = true;
            this.explorerButton.Click += new System.EventHandler(this.explorerButton_Click);
            // 
            // resetTimer
            // 
            this.resetTimer.Interval = 1000;
            this.resetTimer.Tick += new System.EventHandler(this.resetTimer_Tick);
            // 
            // VfsClient
            // 
            this.AcceptButton = this.loginButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(233, 242);
            this.Controls.Add(this.explorerButton);
            this.Controls.Add(this.logoutButton);
            this.Controls.Add(this.loginButton);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.userTextBox);
            this.Controls.Add(this.portNumericUpDown);
            this.Controls.Add(this.ipTextBox);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.userLabel);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.ipLabel);
            this.MaximumSize = new System.Drawing.Size(249, 280);
            this.MinimumSize = new System.Drawing.Size(249, 280);
            this.Name = "VfsClient";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Virtual File System: Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VfsClient_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ipLabel;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox ipTextBox;
        private System.Windows.Forms.NumericUpDown portNumericUpDown;
        private System.Windows.Forms.TextBox userTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.Button logoutButton;
        private System.Windows.Forms.Button explorerButton;
        private System.Windows.Forms.Timer resetTimer;
    }
}

