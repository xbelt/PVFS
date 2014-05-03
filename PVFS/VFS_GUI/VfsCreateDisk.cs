using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VFS.VFS;
using VFS.VFS.Models;

namespace VFS_GUI
{
    public partial class VfsCreateDisk : Form
    {
        public string ResultPath { get; private set; }
        public string ResultName { get; private set; }
        public decimal ResultBlockSize { get; private set; }
        public string ResultPassword { get; private set; }
        public string ResultSize { get; private set; }

        public VfsCreateDisk(bool nonLocal)
        {
            InitializeComponent();
            pathTextBox.Text = Environment.CurrentDirectory;
            this.sizeComboBox.SelectedIndex = 1;
            this.DialogResult = DialogResult.Cancel;
            if (nonLocal)
            {
                this.pathTextBox.Text = "";
                this.pathTextBox.Enabled = false;
                this.browseFolderButton.Enabled = false;
            }
        }

        private void browseFolderButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = @"Select new folder";
                dialog.ShowNewFolderButton = true;
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void abortButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            ResultPath = pathTextBox.Text;
            ResultName = nameTextBox.Text;
            ResultBlockSize = blockSizeNumericUpDown.Value;
            ResultPassword = pwTextBox.Text;
            ResultSize= sizeNumericUpDown.Value.ToString() + sizeComboBox.SelectedItem;

            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
