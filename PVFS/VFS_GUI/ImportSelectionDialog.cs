using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VFS_GUI
{
    public partial class ImportSelectionDialog : Form
    {
        public bool FileSelect { get; private set; }
        public ImportSelectionDialog()
        {
            InitializeComponent();

            DialogResult = DialogResult.Cancel;
        }

        private void FileButton_Click(object sender, EventArgs e)
        {
            FileSelect = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void FolderButton_Click(object sender, EventArgs e)
        {
            FileSelect = false;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
