using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VFS_Network
{
    public partial class AddUserDialog : Form
    {
        public string ResultName { get; private set; }
        public string ResultPassword { get; private set; }


        public AddUserDialog()
        {
            InitializeComponent();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (this.nameTextBox.Text.Length == 0)
            {
                this.nameLabel.ForeColor = Color.Red;
                this.nameTextBox.Focus();
                return;
            }
            if (this.passwordTextBox.Text.Length == 0)
            {
                this.passwordLabel.ForeColor = Color.Red;
                this.passwordTextBox.Focus();
                return;
            }
            if (this.nameTextBox.Text.Length < 128)
            {
                this.ResultName = this.nameTextBox.Text;
                this.ResultPassword = this.passwordTextBox.Text;
                Close();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            this.nameLabel.ForeColor = this.nameTextBox.Text.Length >= 128 || this.nameTextBox.Text.Length == 0 ?
                Color.Red : Color.Black;
        }

        private void passwordTextBox_TextChanged(object sender, EventArgs e)
        {
            this.passwordLabel.ForeColor = this.passwordTextBox.Text.Length == 0 ? Color.Red : Color.Black;
        }
    }
}
