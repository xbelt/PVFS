namespace VFS_GUI
{
    partial class VfsCreateDisk
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
            this.pathLabel = new System.Windows.Forms.Label();
            this.pathTextBox = new System.Windows.Forms.TextBox();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.blockSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.bsLabel = new System.Windows.Forms.Label();
            this.pwLabel = new System.Windows.Forms.Label();
            this.pwTextBox = new System.Windows.Forms.TextBox();
            this.abortButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.siezComboBox = new System.Windows.Forms.ComboBox();
            this.sizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.blockSizeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // pathLabel
            // 
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new System.Drawing.Point(12, 16);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(29, 13);
            this.pathLabel.TabIndex = 0;
            this.pathLabel.Text = "Path";
            // 
            // pathTextBox
            // 
            this.pathTextBox.Location = new System.Drawing.Point(79, 13);
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.Size = new System.Drawing.Size(120, 20);
            this.pathTextBox.TabIndex = 1;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(79, 40);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(120, 20);
            this.nameTextBox.TabIndex = 2;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(12, 43);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 3;
            this.nameLabel.Text = "Name";
            // 
            // blockSizeNumericUpDown
            // 
            this.blockSizeNumericUpDown.Location = new System.Drawing.Point(79, 67);
            this.blockSizeNumericUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.blockSizeNumericUpDown.Name = "blockSizeNumericUpDown";
            this.blockSizeNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this.blockSizeNumericUpDown.TabIndex = 4;
            // 
            // bsLabel
            // 
            this.bsLabel.AutoSize = true;
            this.bsLabel.Location = new System.Drawing.Point(12, 69);
            this.bsLabel.Name = "bsLabel";
            this.bsLabel.Size = new System.Drawing.Size(52, 13);
            this.bsLabel.TabIndex = 5;
            this.bsLabel.Text = "Blocksize";
            // 
            // pwLabel
            // 
            this.pwLabel.AutoSize = true;
            this.pwLabel.Location = new System.Drawing.Point(12, 123);
            this.pwLabel.Name = "pwLabel";
            this.pwLabel.Size = new System.Drawing.Size(53, 13);
            this.pwLabel.TabIndex = 6;
            this.pwLabel.Text = "Password";
            // 
            // pwTextBox
            // 
            this.pwTextBox.Location = new System.Drawing.Point(79, 120);
            this.pwTextBox.Name = "pwTextBox";
            this.pwTextBox.Size = new System.Drawing.Size(120, 20);
            this.pwTextBox.TabIndex = 7;
            // 
            // abortButton
            // 
            this.abortButton.Location = new System.Drawing.Point(113, 146);
            this.abortButton.Name = "abortButton";
            this.abortButton.Size = new System.Drawing.Size(87, 23);
            this.abortButton.TabIndex = 8;
            this.abortButton.Text = "Abort";
            this.abortButton.UseVisualStyleBackColor = true;
            this.abortButton.Click += new System.EventHandler(this.abortButton_Click);
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(15, 147);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(92, 23);
            this.createButton.TabIndex = 9;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // siezComboBox
            // 
            this.siezComboBox.FormattingEnabled = true;
            this.siezComboBox.Items.AddRange(new object[] {
            "kb",
            "mb",
            "gb",
            "tb"});
            this.siezComboBox.Location = new System.Drawing.Point(161, 94);
            this.siezComboBox.Name = "siezComboBox";
            this.siezComboBox.Size = new System.Drawing.Size(38, 21);
            this.siezComboBox.TabIndex = 10;
            // 
            // sizeNumericUpDown
            // 
            this.sizeNumericUpDown.Location = new System.Drawing.Point(79, 94);
            this.sizeNumericUpDown.Name = "sizeNumericUpDown";
            this.sizeNumericUpDown.Size = new System.Drawing.Size(76, 20);
            this.sizeNumericUpDown.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Size";
            // 
            // VfsCreateDisk
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(212, 177);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sizeNumericUpDown);
            this.Controls.Add(this.siezComboBox);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.abortButton);
            this.Controls.Add(this.pwTextBox);
            this.Controls.Add(this.pwLabel);
            this.Controls.Add(this.bsLabel);
            this.Controls.Add(this.blockSizeNumericUpDown);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.pathTextBox);
            this.Controls.Add(this.pathLabel);
            this.Name = "VfsCreateDisk";
            this.Text = "VfsCreateDisk";
            ((System.ComponentModel.ISupportInitialize)(this.blockSizeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.TextBox pathTextBox;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.NumericUpDown blockSizeNumericUpDown;
        private System.Windows.Forms.Label bsLabel;
        private System.Windows.Forms.Label pwLabel;
        private System.Windows.Forms.TextBox pwTextBox;
        private System.Windows.Forms.Button abortButton;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.ComboBox siezComboBox;
        private System.Windows.Forms.NumericUpDown sizeNumericUpDown;
        private System.Windows.Forms.Label label1;
    }
}