﻿using System;
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
        public bool fileselect { get; private set; }

        public ImportSelectionDialog()
        {
            InitializeComponent();

            DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fileselect = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fileselect = false;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
