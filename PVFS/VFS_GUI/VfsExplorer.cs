using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VFS.VFS;
using VFS.VFS.Models;

namespace VFS_GUI
{
    public partial class VfsExplorer : Form
    {
        public static RemoteConsole Console { get; private set; }
        private string Address { get; set; }

        private readonly List<string> _selection;

        /// <summary>
        /// Store files here when copy or cut are pressed.
        /// </summary>
        private List<string> markedFiles;

        /// <summary>
        /// Store if cut or copy was pressed last.
        /// </summary>
        private bool cut;
        private OpenFileDialog importOFD, diskOFD;
        private FolderBrowserDialog folderBD;
        public TreeNode CurrentNode { get; set; }


        public VfsExplorer()
        {
            Console = new RemoteConsole(this);
            VfsManager.Console = new LocalConsole(Console);

            _selection = new List<string>();

            InitializeComponent();

            diskOFD = new OpenFileDialog();
            diskOFD.Filter = "Virtual Disks|*.vdi|All Files|*.*";
            diskOFD.InitialDirectory = Environment.CurrentDirectory;
            diskOFD.Multiselect = false;

            importOFD = new OpenFileDialog();
            importOFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            importOFD.Multiselect = true;

            folderBD = new FolderBrowserDialog();
            folderBD.RootFolder = Environment.SpecialFolder.MyComputer;

            setButtonStates();

            mainTreeView.AfterExpand += handleTreeViewExpand;
            mainTreeView.NodeMouseClick += handleTreeViewMouseClick;

            mainListView.DoubleClick += handleItemDoubleClick;
            mainListView.ItemSelectionChanged += handleItemSelectionClick;

            var toolTips = new ToolTip {AutoPopDelay = 5000, InitialDelay = 500, ReshowDelay = 500, ShowAlways = true};

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
            toolTips.SetToolTip(moveButton, "Cut an entry");
            toolTips.SetToolTip(copyButton, "Copy an entry");
            toolTips.SetToolTip(pasteButton, "Paste an entry");
            toolTips.SetToolTip(deleteButton, "Delete an entry");
        }

        private void handleItemSelectionClick(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                if (!_selection.Contains(e.Item.Text))
                {
                    _selection.Add(VfsManager.WorkingDirectory.AbsolutePath + "/" + e.Item.Text);
                }
            }
            else
            {
                _selection.Remove(e.Item.Text);
            }
            setButtonStates();
        }

        private void handleItemDoubleClick(object sender, EventArgs e)
        {
            if (mainListView.SelectedItems.Count == 1)
            {
                string testPath = Address + "/" + mainListView.SelectedItems[0].Text;
                VfsEntry testEntry = VfsManager.GetEntry(testPath);
                if ( testEntry != null && testEntry.IsDirectory)
                {
                    Address = testPath;
                    addressTextBox.Text = Address;
                    Console.Command("cd " + Address);
                    _selection.Clear();
                    for (int i = 0; i < CurrentNode.Nodes.Count; i++)
                    {
                        if (CurrentNode.Nodes[i].Text == mainListView.SelectedItems[0].Text)
                        {
                            CurrentNode = CurrentNode.Nodes[i];
                        }
                    } 
                }

                UpdateExplorer();
            }
        }
       
        /// <summary>
        /// Navigates to specified path (if it exists)
        /// Updates Explorer and Address
        /// </summary>
        /// <param name="Path">The absolute path we want to go to.</param>
        private void Navigate(string Path)
        {
            //TODO: work around the manager.
            VfsEntry toGo = VfsManager.GetEntry(Path);
            if (toGo != null && toGo.IsDirectory)
            {
                string command = "cd " + Path;
                Address = Path;
                Console.Command(command);
                UpdateExplorer(); 
            }
        }

        private void handleTreeViewMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            CurrentNode = e.Node;
            string path= "/" + e.Node.FullPath;
            addressTextBox.Text = path;
            Navigate(path);
            
            _selection.Clear();
            setButtonStates();
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void UpdateExplorer()
        {
            // ls -> manager
            // 

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

            List<string> disks;
            VfsManager.ListDisks(out disks);
            // check over console.command("ldisks") whether there are some open disks.
            // maybe not every time because this runs a lot (probably after each user action)
            if (disks.Count > 0)//Console.Query("Are there some open disks?", "Yes", "No") == 0)
            {
                foreach (Button b in NeedDisk)
                {
                    b.Enabled = true;
                }

                exportButton.Enabled = _selection.Count != 0;
                moveButton.Enabled = _selection.Count != 0;
                copyButton.Enabled = _selection.Count != 0;
                deleteButton.Enabled = _selection.Count != 0;

                renameButton.Enabled = _selection.Count == 1;

                pasteButton.Enabled = markedFiles != null;
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

        public void setStatus(string status)
        {
            statusBarText.Text = status;
        }

        //---------------Buttons & Co.---------------

        private void createDiskButton_Click(object sender, EventArgs e)
        {
            var window = new VfsCreateDisk();
            if (window.ShowDialog() == DialogResult.OK)
            {
                var path = "";
                var name = "";
                var bs = "";
                var pw = "";

                if (window.ResultPath != Environment.CurrentDirectory)
                {
                    path = " -p " + window.ResultPath;
                }

                if (window.ResultName != "")
                {
                    name = " -n " + window.ResultName;
                }

                if (window.ResultBlockSize != 2048)
                {
                    bs = " -b " + window.ResultBlockSize;
                }

                if (window.ResultPassword != "")
                {
                    pw = " -pw " + window.ResultPassword;
                }

                VfsExplorer.Console.Command("cdisk" + path + name + bs + pw + " -s " + window.ResultSize);

                UpdateExplorer();
            }
        }

        private void openDiskButton_Click(object sender, EventArgs e)
        {
            if (diskOFD.ShowDialog(this) == DialogResult.OK)
            {
                string command = "ldisk " + diskOFD.FileName;

                Console.Command(command);

                List<string> dirs;
                List<string> files;

                var diskName = diskOFD.ToString().Substring(diskOFD.ToString().LastIndexOf("\\") + 1, diskOFD.ToString().Length - 5 - diskOFD.ToString().LastIndexOf("\\"));
                VfsEntry entry;
                while (VfsManager.GetEntryConcurrent("/" + diskName, out entry) != 0) { }
                Address = VfsManager.CurrentDisk.Root.AbsolutePath;
                addressTextBox.Text = Address;
                UpdateExplorer();

                VfsManager.ListEntries(VfsManager.CurrentDisk.Root.AbsolutePath, out dirs, out files);
                var parentNode = mainTreeView.Nodes.Add(VfsManager.CurrentDisk.DiskProperties.Name);

                foreach (var dir in dirs)
                {
                    parentNode.Nodes.Add(dir);
                }
                CurrentNode = parentNode;
                setButtonStates();


                UpdateExplorer();
                
            }
        }

        private void closeDiskButton_Click(object sender, EventArgs e)
        {
            var diskName = "";
            var indexOf = Address.IndexOf("/", 1);
            if (indexOf == -1)
            {
                diskName = Address.Substring(1);
            }
            else
            {
                diskName = Address.Substring(1, indexOf - 1);
            }
            Console.Command("udisk " + diskName);
            TreeNode node = null;
            for (int i = 0; i < mainTreeView.Nodes.Count; i++)
            {
                if (diskName == mainTreeView.Nodes[i].Text)
                {
                    node = mainTreeView.Nodes[i];
                }
            }
            if (node != null)
            {
                mainTreeView.Nodes.Remove(node);
            }
        }

        private void deleteDiskButton_Click(object sender, EventArgs e)
        {
            var diskName = "";
            var indexOf = Address.IndexOf("/", 1);
            if (indexOf == -1)
            {
                diskName = Address.Substring(1);
            }
            else
            {
                diskName = Address.Substring(1, indexOf - 1);
            }
            Console.Command("rmdisk " + diskName);
            TreeNode node = null;
            for (int i = 0; i < mainTreeView.Nodes.Count; i++)
            {
                if (diskName == mainTreeView.Nodes[i].Text)
                {
                    node = mainTreeView.Nodes[i];
                }
            }
            if (node != null)
            {
                mainTreeView.Nodes.Remove(node);
            }
        }
        
        private void createDirectoryButton_Click(object sender, EventArgs e)
        {
            // show name dialog or create a new thingy and allow for a namechange in the list view immediately
            var window = new EnterName();
            if (window.ShowDialog() == DialogResult.OK)
            {
                string command = "mkdir " + Address + "/" + window.Result;
                Console.Command(command);
                //Add only to treeview, if directory was valid and didn't exist already.
                if (VfsManager.GetEntry(Address + "/" + window.Result) != null)
                    CurrentNode.Nodes.Add(window.Result);
            }
        }

        private void createFileButton_Click(object sender, EventArgs e)
        {
            // show name dialog or create a new thingy and allow for a namechange in the list view immediately
            var window = new EnterName();
            if (window.ShowDialog() == DialogResult.OK)
            {
                string command = "mk " + Address + "/" + window.Result;

                Console.Command(command);
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            ImportSelectionDialog isd = new ImportSelectionDialog();

            if (isd.ShowDialog() == DialogResult.OK)
            {
                if (isd.FileSelect)
                {
                    //TODO: attention, will remove invalid characters from the names.
                    if (importOFD.ShowDialog(this) == DialogResult.OK)
                    {
                        foreach (string fileName in importOFD.FileNames)
                        {
                            string command = "im " + fileName + " " + Address + " ";
                            Console.Command(command);
                        }
                    }
                }
                else
                {
                    //folder...
                     if (folderBD.ShowDialog(this) == DialogResult.OK)
                     {
                         string command = "im " + folderBD.SelectedPath + " " + Address;
                         Console.Command(command);
                     }
                }
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            //TODO: show folder select dialog
            string dst = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            foreach (string file in _selection)
            {
                string command = "ex " + file + " " + dst;

                Console.Command(command);
            }
        }

        private void renameButton_Click(object sender, EventArgs e)
        {
            if (_selection.Count != 1)
                throw new ArgumentException("Button should not be pressable when not exactly 1 file is selected.");

            // show name dialog or allow for a namechange in the list view
            var window = new EnterName();
            if (window.ShowDialog() == DialogResult.OK)
            {
                string command = "rn " + _selection[0] + " " + window.Result;

                Console.Command(command);
            }
        }

        private void moveButton_Click(object sender, EventArgs e)
        {
            if (_selection.Count == 0)
                throw new ArgumentException("Button should not be pressable when nothing is selected.");

            cut = true;
            markedFiles = new List<string>(_selection);
            _selection.Clear();
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            if (_selection.Count == 0)
                throw new ArgumentException("Button should not be pressable when nothing is selected.");

            cut = false;
            markedFiles = new List<string>(_selection);
            _selection.Clear();
        }

        private void pasteButton_Click(object sender, EventArgs e)
        {
            if (markedFiles == null)
                throw new ArgumentException("Button sould not be pressable when nothing was copied/cut.");

            if (cut)
            {
                foreach (string file in markedFiles)
                {
                    string command = "mv " + file + " " + Address + " ";

                    Console.Command(command);
                }
                markedFiles = null;
            }
            else
            {
                foreach (string file in markedFiles)
                {
                    string command = "cp " + file + " " + Address + " ";
                    Console.Command(command);
                }
                // not clearing markedfiles = paste multiple copies.
            }
        }
        
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (_selection.Count == 0)
                throw new ArgumentException("Button should not be pressable when nothing is selected.");

            string command = "";

            foreach (string s in _selection)
            {
                command += "rm " + _selection[0] + " ";
            }
            Console.Command(command);
        }

        private void VfsExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.Command("quit");
        }

        //TODO: what's the use of this?
        private void addressTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void addressTextBox_EnterUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) //Notice: removing this 'if' will lead to instant navigation (which is pretty cool imo)
            {
                //TODO: somehow work around this without using VfsManager directly.
                VfsEntry toGo = VfsManager.GetEntry(this.addressTextBox.Text);
                if (toGo != null && toGo.IsDirectory)
                {
                    string path = this.addressTextBox.Text;
                    Navigate(path);
                } 
            }
        }

    }
}
