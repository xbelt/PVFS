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

namespace VFS_GUI
{
    public partial class VfsCreateDisk : Form
    {
        private bool _isPathChange = true;

        public VfsCreateDisk()
        {
            InitializeComponent();
            pathTextBox.Text = Environment.CurrentDirectory;
            blockSizeNumericUpDown.Value = 2048;
            pathTextBox.GotFocus += OpenPathDialog;
        }

        private void OpenPathDialog(object sender, EventArgs e)
        {
            if (_isPathChange)
            {
                _isPathChange = false;
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
                new Thread(() =>
                {
                    Thread.Sleep(100); 
                    _isPathChange = true;
                }).Start();
            }
        }

        private void abortButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            var path = "";
            var name = "";
            var bs = "";
            var pw = "";
            var size = "";

            if (pathTextBox.Text != Environment.CurrentDirectory)
            {
                path = " -p " + pathTextBox.Text;
            }

            if (nameTextBox.Text != "")
            {
                name = " -n " + nameTextBox.Text;
            }

            if (blockSizeNumericUpDown.Value != 2048)
            {
                bs = " -b " + blockSizeNumericUpDown.Value;
            }

            if (pwTextBox.Text != "")
            {
                pw = " -pw " + pwTextBox.Text;
            }

            if (sizeNumericUpDown.Value <= 0)
            {
                MessageBox.Show("Please enter a valid size of the disk");
                return;
            }

            VfsExplorer.Console.Command("cdisk" + path + name + bs + pw + " -s " + sizeNumericUpDown.Value + siezComboBox.SelectedValue);

        }
    }
}
