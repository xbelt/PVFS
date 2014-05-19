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
    public partial class AdvancedSearch : Form
    {
        private const int OptionsLength = 5;

        public String Term;
        public bool[] Options;
        public AdvancedSearch()
        {
            InitializeComponent();

            Options = new bool[OptionsLength];
        }

        private void AdvancedSearchButton_Click(object sender, EventArgs e)
        {
            Term = this.SearchTermBox.Text;
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void SearchTermBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void SearchTermBox_Click(object sender, EventArgs e)
        {
            this.SearchTermBox.SelectAll();
        }

        private void caseSensitiveBox_CheckedChanged(object sender, EventArgs e)
        {
            Options[0] = this.caseSensitiveBox.Checked;
        }

        private void metricDistanceBox_CheckedChanged(object sender, EventArgs e)
        {
            Options[1] = this.metricDistanceBox.Checked;
        }

        private void onlyFilesBox_CheckedChanged(object sender, EventArgs e)
        {
            Options[2] = this.onlyFilesBox.Checked;
        }

        private void onlyFoldersBox_CheckedChanged(object sender, EventArgs e)
        {
            Options[3] = this.onlyFoldersBox.Checked;
        }

        private void regexChechBox_CheckedChanged(object sender, EventArgs e)
        {
            Options[4] = this.onlyFoldersBox.Checked;
        }
    }
}
