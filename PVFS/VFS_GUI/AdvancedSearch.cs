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
        public AdvancedSearch()
        {
            InitializeComponent();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void AdvancedSearchButton_Click(object sender, EventArgs e)
        {
            String searchText = this.SearchTermBox.Text;
            ((VfsExplorer) this.Parent).Search(searchText); //TODO: this is kinda ugly...?
        }

        private void SearchTermBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void SearchTermBox_Click(object sender, EventArgs e)
        {
            this.SearchTermBox.SelectAll();
        }
    }
}
