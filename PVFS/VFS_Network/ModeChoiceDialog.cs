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
    public partial class ModeChoiceDialog : Form
    {
        public bool Server { get; private set; }

        public ModeChoiceDialog()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        private void serverButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Server = true;
            this.Close();
        }

        private void clientButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Server = false;
            this.Close();
        }

    }
}
