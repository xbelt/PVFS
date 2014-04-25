using System;
using System.Windows.Forms;

namespace VFS_GUI
{
    public partial class EnterName : Form
    {
        public string Result { get; set; }
        public EnterName()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (!VFS.VFS.Models.VfsFile.ValidName(nameTextBox.Text))
            {
                MessageBox.Show("Allowed characters: A-Z a-z 0-9 . _");
                return;
            }

            Result = nameTextBox.Text;
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
