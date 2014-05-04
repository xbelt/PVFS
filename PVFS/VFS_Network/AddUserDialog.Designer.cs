namespace VFS_Network
{
    partial class AddUserDialog
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
            this.nameLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(12, 9);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(63, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "User Name:";
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.passwordLabel.Location = new System.Drawing.Point(12, 41);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(56, 13);
            this.passwordLabel.TabIndex = 2;
            this.passwordLabel.Text = "Password:";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(116, 66);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(197, 66);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(91, 6);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(181, 20);
            this.nameTextBox.TabIndex = 1;
            this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(91, 38);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(181, 20);
            this.passwordTextBox.TabIndex = 3;
            this.passwordTextBox.TextChanged += new System.EventHandler(this.passwordTextBox_TextChanged);
            // 
            // AddUserDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(284, 101);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.nameLabel);
            this.MaximumSize = new System.Drawing.Size(300, 139);
            this.MinimumSize = new System.Drawing.Size(300, 139);
            this.Name = "AddUserDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AddUserDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
    }
}