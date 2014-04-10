using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VFS.VFS;

namespace VFS_GUI
{
    public partial class VfsExplorer : Form
    {
        public LocalConsole Console { get; private set; }
        public string Address { get; private set; }

        private List<string> selection;

        /// <summary>
        /// Store files here when copy or cut are pressed.
        /// </summary>
        private List<string> markedFiles;
        /// <summary>
        /// Store if cut or copy was pressed last.
        /// </summary>
        private bool cut;

        private OpenFileDialog importOFD, diskOFD;


        public VfsExplorer()
        {
            InitializeComponent();

            this.diskOFD = new OpenFileDialog();
            this.diskOFD.Filter = "Virtual Disks|*.vdi|All Files|*.*";
            this.diskOFD.InitialDirectory = Environment.CurrentDirectory;
            this.diskOFD.Multiselect = false;

            this.importOFD = new OpenFileDialog();
            this.importOFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.importOFD.Multiselect = true;
        }


        private void setButtonStates()
        {
            Button[] NeedDisk =
            {
                closeDiskButton, deleteDiskButton, createDirectoryButton, createFileButton,
                importButton, exportButton, renameButton, moveButton, copyButton, pasteButton, deleteButton
            };

            // check over console.command("ldisks") whether there are some open disks.
            // maybe not every time because this is runs a lot (probably after each user action)
            if (Console.Query("Hello", "Yes", "No") == 0)
            {
                foreach (Button b in NeedDisk)
                {
                    b.Enabled = true;
                }

                this.exportButton.Enabled = this.selection.Count != 0;
                this.moveButton.Enabled = this.selection.Count != 0;
                this.copyButton.Enabled = this.selection.Count != 0;
                this.deleteButton.Enabled = this.selection.Count != 0;

                this.renameButton.Enabled = this.selection.Count == 1;

                this.pasteButton.Enabled = this.markedFiles != null;
            }
            else
            {
                foreach (Button b in NeedDisk)
                {
                    b.Enabled = false;
                }
            }


        }




        private void createDiskButton_Click(object sender, EventArgs e)
        {
            // Show createDisk window
        }

        private void openDiskButton_Click(object sender, EventArgs e)
        {
            if (this.diskOFD.ShowDialog(this) == DialogResult.OK)
            {
                string command = "ldisk " + this.diskOFD.FileName;

                Console.Command(command);
            }
        }

        private void closeDiskButton_Click(object sender, EventArgs e)
        {

        }

        private void deleteDiskButton_Click(object sender, EventArgs e)
        {

        }

        private void createDirectoryButton_Click(object sender, EventArgs e)
        {
            // show name dialog
            string name = "test";

            string command = "mkdir " + Address + "/" + name;

            Console.Command(command);
        }

        private void createFileButton_Click(object sender, EventArgs e)
        {
            // show name dialog
            string name = "test.fail";

            string command = "mk " + Address + "/" + name;

            Console.Command(command);
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            // whoops this doesn't work with directories.
            if (this.importOFD.ShowDialog(this) == DialogResult.OK) {

                foreach (string fileName in this.importOFD.FileNames)
                {
                    string command = "im " + fileName + " " + this.Address;

                    Console.Command(command);
                }
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            // show folder select dialog
            string dst = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            foreach (string file in this.selection)
            {
                string command = "ex " + file + " " + dst;

                Console.Command(command);
            }
        }

        private void renameButton_Click(object sender, EventArgs e)
        {
            if (selection.Count != 1)
                throw new ArgumentException("Button should not be pressable when not exactly 1 file is selected.");

            // show name dialog
            string newname = "test2.fail";

            string command = "rn " + selection[0] + " " + newname;

            Console.Command(command);
        }


        private void moveButton_Click(object sender, EventArgs e)
        {
            if (this.selection.Count == 0)
                throw new ArgumentException("Button should not be pressable when nothing is selected.");

            this.cut = true;
            this.markedFiles = new List<string>(this.selection);
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            if (this.selection.Count == 0)
                throw new ArgumentException("Button should not be pressable when nothing is selected.");

            this.cut = true;
            this.markedFiles = new List<string>(this.selection);
        }

        private void pasteButton_Click(object sender, EventArgs e)
        {
            if (this.markedFiles == null)
                throw new ArgumentException("Button sould not be pressable when nothing was copied/cut.");

            if (this.cut)
            {
                foreach (string file in this.markedFiles)
                {
                    string command = "mv " + file + " " + Address;

                    Console.Command(command);
                }

                this.markedFiles = null;
            }
            else
            {
                foreach (string file in this.markedFiles)
                {
                    string command = "cp " + file + " " + Address;

                    Console.Command(command);
                }
                // not clearing markedfiles = paste multiple copies.
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (this.selection.Count == 0)
                throw new ArgumentException("Button should not be pressable when nothing is selected.");

            foreach (string file in this.selection)
            {
                string command = "rm " + file;

                Console.Command(command);
            }
        }

    }
}
