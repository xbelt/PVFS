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
using VFS.VFS;

namespace VFS_GUI
{
    public partial class VfsExplorer : Form
    {
        public static LocalConsole Console { get; private set; }
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
        public TreeNode CurrentNode { get; set; }


        public VfsExplorer()
        {
            Console = new LocalConsole(this);
            this.selection = new List<string>();

            InitializeComponent();

            this.diskOFD = new OpenFileDialog();
            this.diskOFD.Filter = "Virtual Disks|*.vdi|All Files|*.*";
            this.diskOFD.InitialDirectory = Environment.CurrentDirectory;
            this.diskOFD.Multiselect = false;

            this.importOFD = new OpenFileDialog();
            this.importOFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.importOFD.Multiselect = true;

            setButtonStates();

            mainTreeView.AfterExpand += handleTreeViewExpand;
            mainTreeView.NodeMouseClick += handleTreeViewMouseClick;

            mainListView.DoubleClick += handleItemDoubleClick;

            var toolTips = new ToolTip {AutoPopDelay = 5000, InitialDelay = 1000, ReshowDelay = 500, ShowAlways = true};

            // Set up the ToolTip text for the Button and Checkbox.
            toolTips.SetToolTip(createDiskButton, "Create a new Disk");
            toolTips.SetToolTip(openDiskButton, "Open a disk");
            toolTips.SetToolTip(closeDiskButton, "Unload a disk");
            toolTips.SetToolTip(deleteDiskButton, "Delete a disk");
            toolTips.SetToolTip(createDirectoryButton, "Create a new directory");
            toolTips.SetToolTip(createFileButton, "Create a new file");
            toolTips.SetToolTip(importButton, "Import a file from the host filesystem");
            toolTips.SetToolTip(exportButton, "Export a file to the host filesystem");
            toolTips.SetToolTip(renameButton, "Rename an entry");
            toolTips.SetToolTip(moveButton, "Move an entry");
            toolTips.SetToolTip(copyButton, "Copy an entry");
            toolTips.SetToolTip(pasteButton, "Paste an entry");
            toolTips.SetToolTip(deleteButton, "Delete an entry");
        }

        private void handleItemDoubleClick(object sender, EventArgs e)
        {
            if (mainListView.SelectedItems.Count == 1)
            {
                Address = Address + "/" + mainListView.SelectedItems[0].Text;
                addressTextBox.Text = Address;
                Console.Command("cd " + Address);

                for (int i = 0; i < CurrentNode.Nodes.Count; i++)
                {
                    if (CurrentNode.Nodes[i].Text == mainListView.SelectedItems[0].Text)
                    {
                        CurrentNode = CurrentNode.Nodes[i];
                    }
                }

                UpdateContent();
            }
        }

        private void handleTreeViewMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            CurrentNode = e.Node;
            Address = "/" + e.Node.FullPath;
            addressTextBox.Text = Address;
            Console.Command("cd " + Address);

            UpdateContent();
        }

        private void UpdateContent()
        {
            List<string> dirs;
            List<string> files;

            VfsManager.ListEntries(Address, out dirs, out files);

            SetContent(dirs, files);
        }


        private void handleTreeViewExpand(object sender, TreeViewEventArgs e)
        {
            for (int i = 0; i < e.Node.Nodes.Count; i++)
            {
                List<string> dirs;
                List<string> files;

                VfsManager.ListEntries("/" + e.Node.Nodes[i].FullPath, out dirs, out files);

                foreach (var dir in dirs)
                {
                    e.Node.Nodes[i].Nodes.Add(dir);
                }
            }
        }


        private void setButtonStates()
        {
            Button[] NeedDisk =
            {
                closeDiskButton, deleteDiskButton, createDirectoryButton, createFileButton,
                importButton, exportButton, renameButton, moveButton, copyButton, pasteButton, deleteButton
            };

            // check over console.command("ldisks") whether there are some open disks.
            // maybe not every time because this runs a lot (probably after each user action)
            if (Console.Query("Are there some open disks?", "Yes", "No") == 0)
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


        public void SetContent(List<string> directories, List<string> files)
        {
            mainListView.Items.Clear();
            foreach (var directory in directories)
            {
                var item = mainListView.Items.Add(directory, 0);
            }

            foreach (var file in files)
            {
                var item = mainListView.Items.Add(file, 1);
            }
        }

        //---------------Buttons & Co.---------------

        private void createDiskButton_Click(object sender, EventArgs e)
        {
            var window = new VfsCreateDisk(this);
            window.Show();
        }

        private void openDiskButton_Click(object sender, EventArgs e)
        {
            if (this.diskOFD.ShowDialog(this) == DialogResult.OK)
            {
                string command = "ldisk " + this.diskOFD.FileName;

                Console.Command(command);

                List<string> dirs;
                List<string> files;
                
                while(VfsManager.CurrentDisk == null) {}

                VfsManager.ListEntries(VfsManager.CurrentDisk.Root.AbsolutePath, out dirs, out files);
                var parentNode = mainTreeView.Nodes.Add(VfsManager.CurrentDisk.DiskProperties.Name);

                Address = VfsManager.CurrentDisk.Root.AbsolutePath;
                addressTextBox.Text = Address;
                foreach (var dir in dirs)
                {
                    parentNode.Nodes.Add(dir);
                }

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
            // show name dialog or create a new thingy and allow for a namechange in the list view immediately
            var window = new EnterName();
            window.ShowDialog();
            if (window.Cancelled == false)
            {
                string command = "mkdir " + Address + "/" + window.Result;

                Console.Command(command);

                CurrentNode.Nodes.Add(window.Result);
            }
        }

        private void createFileButton_Click(object sender, EventArgs e)
        {
            // show name dialog or create a new thingy and allow for a namechange in the list view immediately
            var window = new EnterName();
            window.ShowDialog();
            if (window.Cancelled == false)
            {
                string command = "mk " + Address + "/" + window.Result;

                Console.Command(command);
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            // whoops this doesn't work with directories.
            if (this.importOFD.ShowDialog(this) == DialogResult.OK)
            {

                foreach (string fileName in this.importOFD.FileNames)
                {
                    string command = "im " + fileName + " " + Address;

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

            // show name dialog or allow for a namechange in the list view
            var window = new EnterName();
            window.ShowDialog();
            if (window.Cancelled == false)
            {
                string command = "rn " + selection[0] + " " + window.Result;

                Console.Command(command);
            }
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

            string command = this.selection.Aggregate("", (agg, file) => agg + "rm " + file + " ");

            Console.Command(command);
        }

        private void VfsExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.Command("quit");
        }
    }
}
