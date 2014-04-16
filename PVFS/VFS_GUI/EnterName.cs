using System;
using System.Windows.Forms;

namespace VFS_GUI
{
    public partial class EnterName : Form
    {
        public string Result { get; set; }
        public bool Cancelled { get; set; }
        public EnterName()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Result = nameTextBox.Text;
            Cancelled = false;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Cancelled = true;
            Close();
        }
    }
}
