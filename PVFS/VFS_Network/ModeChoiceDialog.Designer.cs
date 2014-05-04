namespace VFS_Network
{
    partial class ModeChoiceDialog
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
            this.serverButton = new System.Windows.Forms.Button();
            this.clientButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // serverButton
            // 
            this.serverButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serverButton.Location = new System.Drawing.Point(12, 12);
            this.serverButton.Name = "serverButton";
            this.serverButton.Size = new System.Drawing.Size(253, 52);
            this.serverButton.TabIndex = 0;
            this.serverButton.Text = "Start as Server";
            this.serverButton.UseVisualStyleBackColor = true;
            this.serverButton.Click += new System.EventHandler(this.serverButton_Click);
            // 
            // clientButton
            // 
            this.clientButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clientButton.Location = new System.Drawing.Point(12, 70);
            this.clientButton.Name = "clientButton";
            this.clientButton.Size = new System.Drawing.Size(253, 52);
            this.clientButton.TabIndex = 1;
            this.clientButton.Text = "Start as Client";
            this.clientButton.UseVisualStyleBackColor = true;
            this.clientButton.Click += new System.EventHandler(this.clientButton_Click);
            // 
            // ModeChoiceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 133);
            this.Controls.Add(this.clientButton);
            this.Controls.Add(this.serverButton);
            this.MaximumSize = new System.Drawing.Size(293, 171);
            this.MinimumSize = new System.Drawing.Size(293, 171);
            this.Name = "ModeChoiceDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Virtual File System";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button serverButton;
        private System.Windows.Forms.Button clientButton;
    }
}