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
        public const string Caption = "Virtual File System";

        public static RemoteConsole Console { get; private set; }
        public bool Ready { get; private set; }
        public bool NonLocal { get; set; }
        private string Address { get; set; }
        private List<string> Disks { get; set; }

        private string[] selectedNames;
        private List<string> selectedPaths
        {
            get
            {
                if (this.Address == "/")
                    return new List<string>();
                else
                    return selectedNames.Select(n => this.Address + "/" + n).ToList();
            }
        }

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

        private Object LoadedObject = new Object();


        public VfsExplorer(RemoteConsole console)
        {
            Console = console;
            Console.setExplorer(this);

            this.Ready = true;
            this.NonLocal = false;
            this.Address = "/";
            this.Disks = new List<string>();
            this.selectedNames = new string[0];

            InitializeComponent();

            diskOFD = new OpenFileDialog();
            diskOFD.Filter = "Virtual Disks|*.vdi|All Files|*.*";
            diskOFD.InitialDirectory = Environment.CurrentDirectory;
            diskOFD.Multiselect = false;

            importOFD = new OpenFileDialog();
            importOFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            importOFD.Multiselect = true;

            folderBD = new FolderBrowserDialog();
            folderBD.RootFolder = Environment.SpecialFolder.Desktop;

            setButtonStates();

            #region Tooltips
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
            #endregion
        }

        //---------------Set Content---------------

        public void UpdateExplorer(bool disks)
        {
            if (disks)
                Console.Command("ldisks");

            if (this.Address != "/")
                Console.Command("ls " + this.Address);
        }

        private void Navigate(string path)
        {
            if (!path.StartsWith("/")) return;

            if (path != this.Address)
            {
                this.mainListView.SelectedIndices.Clear();
            }

            this.Address = path;

            this.addressTextBox.Text = path;

            this.mainListView.Enabled = false;

            this.UpdateExplorer(path == "/");
        }

        public void ReceiveListEntries(string path, string[] directories, string[] files)
        {
            if (path == this.Address)
            {
                this.mainListView.Enabled = true;
                this.mainListView.MultiSelect = true;

                List<string> sel = this.selectedNames.ToList();

                mainListView.Items.Clear();
                foreach (var directory in directories)
                {
                    mainListView.Items.Add(directory, directory, 0);
                }

                foreach (var file in files)
                {
                    mainListView.Items.Add(file, file, 1);
                }

                // Restore Selection
                mainListView.SelectedIndices.Clear();
                foreach (string name in sel)
                {
                    if (mainListView.Items.ContainsKey(name))
                        mainListView.SelectedIndices.Add(mainListView.Items.IndexOfKey(name));
                }

                setButtonStates();
            }

            #region Update Tree
            string[] names;
            TreeNode last;
            TreeNode node = getNode(path, out last, out names);

            if (node == null)
            {
                int i = 0;
                if (last == null)
                {
                    last = this.mainTreeView.Nodes.Add(names[0], names[0]);
                    i++;
                }

                for (; i < names.Length; i++)
                {
                    last = last.Nodes.Add(names[i], names[i]);
                }

                node = last;
            }

            node.Tag = LoadedObject;
            treeSync(node.Nodes, directories);
            node.Expand();
            mainTreeView.SelectedNode = node;
            #endregion
        }

        public void ReceiveListDisks(string[] disks)
        {
            this.Disks = disks.ToList();

            treeSync(this.mainTreeView.Nodes, disks);

            if (this.Address == "/")
            {
                this.mainListView.Enabled = true;
                this.mainListView.MultiSelect = false;

                List<string> sel = this.selectedNames.ToList();

                mainListView.Items.Clear();
                foreach (var disk in disks)
                {
                    mainListView.Items.Add(disk, disk, 2);
                }

                // Restore Selection
                mainListView.SelectedIndices.Clear();
                foreach (string name in sel)
                {
                    if (mainListView.Items.ContainsKey(name))
                        mainListView.SelectedIndices.Add(mainListView.Items.IndexOfKey(name));
                }
            }


            setButtonStates();
        }

        public void ReceivedError(string command, string message)
        {
            if (command.StartsWith("ls"))
            {
                var path = command.Substring(3);
                if (path == this.Address)
                {
                    string newpath = path.Remove(path.LastIndexOf('/'));
                    if (newpath == "")
                        newpath = "/";
                    Navigate(newpath);
                }
            }
            else
            {
                UpdateExplorer(false);

                MessageBox.Show(this, message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetStatus(string comm, string status)
        {
            string[] updateCommands = new string[] {
                "mkdir",
                "mk",
                "im",
                "rn",
                "mv",
                "cp"
            };

            foreach (string s in updateCommands)
            {
                if (comm.StartsWith(s))
                {
                    UpdateExplorer(false);
                    break;
                }
            }

            statusBarText.Text = status;
        }

        public void SetReady()
        {
            this.statusWorkingText.Text = "Ready";
        }

        public void SetBusy()
        {
            this.statusWorkingText.Text = "Busy";
        }

        private void setButtonStates()
        {
            Button[] NeedDisk =
            {
                closeDiskButton, deleteDiskButton, createDirectoryButton, createFileButton,
                importButton, exportButton, renameButton, moveButton, copyButton, pasteButton, deleteButton
            };

            if (Disks.Count > 0 && Address != "/")
            {
                foreach (Button b in NeedDisk)
                {
                    b.Enabled = true;
                }

                exportButton.Enabled = selectedNames.Length != 0;
                moveButton.Enabled = selectedNames.Length != 0;
                copyButton.Enabled = selectedNames.Length != 0;
                deleteButton.Enabled = selectedNames.Length != 0;

                renameButton.Enabled = selectedNames.Length == 1;

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

        private string getPath(TreeNode node)
        {
            string path = node.Name;
            while (node.Parent != null)
            {
                node = node.Parent;
                path = node.Name + "/" + path;
            }
            return "/" + path;
        }
        private TreeNode getNode(string path)
        {
            String[] remaining;
            TreeNode node;
            return getNode(path, out node, out remaining);
        }
        private TreeNode getNode(string path, out TreeNode current, out string[] remaining)
        {
            var names = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            remaining = names;
            current = null;
            if (names.Length == 0) return null;
            if (!this.mainTreeView.Nodes.ContainsKey(names[0])) return null;
            current = this.mainTreeView.Nodes[names[0]];
            for (int i = 1; i < names.Length; i++)
            {
                if (!current.Nodes.ContainsKey(names[i]))
                {
                    remaining = names.Skip(i).ToArray();
                    return null;
                }

                current = current.Nodes[names[i]];
            }
            remaining = null;
            return current;
        }

        private void treeSync(TreeNodeCollection nodes, string[] dirs)
        {
            foreach (string name in dirs)
            {
                if (!nodes.ContainsKey(name))
                    nodes.Add(name, name);
            }

            List<TreeNode> rem = new List<TreeNode>();
            foreach (TreeNode el in nodes)
            {
                if (!dirs.Contains(el.Text))
                    rem.Add(el);
            }
            foreach (TreeNode el in rem)
            {
                nodes.Remove(el);
            }
        }

        //---------------Navigation & Co.---------------

        private void addressTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigate(this.addressTextBox.Text);
                e.Handled = true;
            }
        }

        private void mainListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.selectedNames = new string[this.mainListView.SelectedItems.Count];
            for(int i = 0; i < this.selectedNames.Length; i++){
                selectedNames[i] = this.mainListView.SelectedItems[i].Text;
            }

            this.setButtonStates();
        }

        private void mainListView_DoubleClick(object sender, EventArgs e)
        {
            if (this.selectedNames.Length == 1)
            {
                if (this.Address == "/")
                    Navigate("/" + this.selectedNames[0]);
                else
                    Navigate(this.Address + "/" + this.selectedNames[0]);
            }
        }

        private void mainListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.mainListView_DoubleClick(sender, EventArgs.Empty);
            else if (e.KeyCode == Keys.Back)
                Navigate(Address.Substring(0, Address.LastIndexOf('/')));
        }

        private void mainTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse)
                Navigate(getPath(e.Node));
        }

        private void mainTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Tag != LoadedObject)
            {
                Console.Command("ls " + getPath(e.Node));
                e.Cancel = true;
            }
        }

        private void mainTreeView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Navigate(getPath(this.mainTreeView.SelectedNode));
        }

        //---------------Buttons & Co.---------------

        private void createDiskButton_Click(object sender, EventArgs e)
        {
            var window = new VfsCreateDisk(this.NonLocal);
            if (window.ShowDialog() == DialogResult.OK)
            {
                var path = "";
                var name = "";
                var bs = "";
                var pw = "";

                if (window.ResultPath != Environment.CurrentDirectory && !this.NonLocal)
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

                UpdateExplorer(true);
            }
        }

        private void openDiskButton_Click(object sender, EventArgs e)
        {
            if (!this.NonLocal)
            {
                if (diskOFD.ShowDialog(this) == DialogResult.OK)
                {
                    string command = "ldisk " + diskOFD.FileName;

                    Console.Command(command);

                    UpdateExplorer(true);
                }
            }
            else
            {
                EnterName dialog = new EnterName();
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string command = "ldisk " + dialog.Result;

                    Console.Command(command);

                    UpdateExplorer(true);
                }
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

            Navigate("/");
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
            if (MessageBox.Show("Do you really want to delete disk " + diskName + "?", Caption, MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Console.Command("rmdisk " + diskName);

                Navigate("/");
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

                if (!this.mainListView.Items.ContainsKey(window.Result))
                {
                    this.mainListView.Items.Add(window.Result, window.Result, 0);

                    TreeNode last;
                    string[] names;
                    TreeNode node = getNode(this.Address, out last, out names);
                    if (node != null)
                        node.Nodes.Add(window.Result, window.Result);
                }
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

                if (!this.mainListView.Items.ContainsKey(window.Result))
                    this.mainListView.Items.Add(window.Result, window.Result, 1);
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            ImportSelectionDialog isd = new ImportSelectionDialog();
            int countValid = 0;
            int total = 0;

            if (isd.ShowDialog() == DialogResult.OK)
            {
                if (isd.FileSelect)
                {
                    if (importOFD.ShowDialog(this) == DialogResult.OK)
                    {
                        foreach (string fileName in importOFD.FileNames)
                        {
                            if (import_NameCheck(fileName))
                            {
                                countValid++; total++;
                                SetStatus("", "Importing " + fileName.Substring(fileName.LastIndexOf('\\')+1) + ".");
                                string command = "im " + fileName + " " + Address;
                                Console.Command(command);
                            }
                            else
                            {
                                SetStatus("", "Invalid characters in " + fileName);
                                total++;
                            }
                        }
                        SetStatus("", countValid + "files of " + total + " have been imported.");
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

        private bool import_NameCheck(string filePath)
        {
            return VfsFile.ValidName(filePath.Substring(filePath.LastIndexOf('\\') + 1));
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (folderBD.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string file in this.selectedPaths)
                {
                    string command = "ex " + file + " " + folderBD.SelectedPath;

                    Console.Command(command);
                }
            }
        }

        private void renameButton_Click(object sender, EventArgs e)
        {
            if (this.selectedNames.Length != 1)
                throw new ArgumentException("Button should not be pressable when not exactly 1 file is selected.");

            var window = new EnterName();
            if (window.ShowDialog() == DialogResult.OK)
            {
                string command = "rn " + selectedPaths[0] + " " + window.Result;

                Console.Command(command);

                var el = this.mainListView.SelectedItems[0];
                this.mainListView.Items.Remove(el);
                this.mainListView.Items.Add(window.Result, window.Result, el.ImageIndex);
            }
        }

        private void moveButton_Click(object sender, EventArgs e)
        {
            if (selectedNames.Length == 0)
                throw new ArgumentException("Button should not be pressable when nothing is selected.");

            cut = true;
            markedFiles = new List<string>(selectedPaths);
            this.mainListView.SelectedIndices.Clear();

            setButtonStates();
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            if (this.selectedNames.Length == 0)
                throw new ArgumentException("Button should not be pressable when nothing is selected.");

            cut = false;
            markedFiles = new List<string>(this.selectedPaths);

            setButtonStates();
        }

        private void pasteButton_Click(object sender, EventArgs e)
        {
            if (markedFiles == null)
                throw new ArgumentException("Button sould not be pressable when nothing was copied/cut.");
            string command = "";
            if (cut)
            {
                foreach (string file in markedFiles)
                {
                    command += "mv " + file + " " + Address + " ";

                    TreeNode last;
                    string[] names;
                    TreeNode node = getNode(file, out last, out names);
                    if (node != null)
                        node.Remove();
                }

                markedFiles = null;
            }
            else
            {
                foreach (string file in markedFiles)
                {
                    command += "cp " + file + " " + Address + " ";
                }
                // not clearing markedfiles = paste multiple copies.
            }
            Console.Command(command);
        }
        
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (this.selectedNames.Length == 0)
                throw new ArgumentException("Button should not be pressable when nothing is selected.");

            string command = "";

            foreach (string s in this.selectedPaths)
            {
                command += "rm " + s + " ";
            }
            Console.Command(command);

            TreeNode last;
            string[] names;
            TreeNode currentNode = getNode(this.Address, out last, out names);
            foreach (string name in selectedNames)
            {
                this.mainListView.Items.RemoveByKey(name);
                if (currentNode != null && currentNode.Nodes.ContainsKey(name))
                    currentNode.Nodes.RemoveByKey(name);
            }
        }

        private void VfsExplorer_Load(object sender, EventArgs e)
        {
            UpdateExplorer(true);
        }

        private void VfsExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Ready = false;
            Console.Command("quit");
        }

        //---------------ListView Drag & Drop---------------//

        private void mainListView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && this.Address != "/")
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void mainListView_DragDrop(object sender, DragEventArgs e)
        {
            //--------------- NEVER ENTERS HERE. WHY????????? -------------- //

            string command = "";

            // It enters here if you use host -> vfs.
            // Anyways, execute only one of these three possibilities.
            // Example d&d from listview to listview folder:
            // start drag by adding an additional dataformat to e.dataFormatPresentWhatever (which can only be used by this application)
            // check if this format is available on drop
            // YES -> intern d&d, use copy/move
            // NO -> use im

            // for "export" drag drop: check if the format is not intern format.
            // YES -> export to temp, give that path as file and op = move
            // NO -> do nothing

            #region Export
            /*
            string tmpPath = System.IO.Path.GetTempPath();
            string[] tmpFilesPath = new string[selectedNames.Length];

            if (selectedNames.Length > 0)
            {
                for (int i = 0; i < selectedNames.Length; i++)
                {
                    tmpFilesPath[i] = tmpPath + "\\" + selectedNames[i];
                    string filePath = selectedPaths[i];
                    command += "export " + filePath + " " + tmpPath + " ";
                    Console.Message("", "exporting " + selectedNames[i]);
                }
                Console.Command(command);
            }
            DataObject dataObject = new DataObject(DataFormats.FileDrop, tmpFilesPath);
            this.mainListView.DoDragDrop(dataObject, DragDropEffects.Copy);
            foreach (string fileName in selectedNames)
            {
                System.IO.File.Delete(tmpPath + "\\" + fileName);
            }*/
            #endregion

            #region Import
            if (selectedNames.Length == 0)
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                foreach (string fileName in files)
                {
                    command = "im " + fileName + " " + Address + " ";
                    Console.Command(command);
                }
            }
            #endregion

            #region Copy in Listview
            //Copy in ListView itself --> TODO: Prevent import when items selected.
            /*
            if (selectedNames.Length > 0)
            {
                var entry = this.mainListView.GetItemAt(e.X, e.Y);

                if (entry != null)
                {
                    TreeNode node;
                    string[] remaining;
                    string dirName = entry.Name;
                    node = getNode(Address + "/" + entry.Name, out node, out remaining);
                    if (node != null) //node was directory
                    {
                        foreach (string fp in selectedPaths)
                        {
                            command += "cp " + fp + " " + Address + "/" + dirName + " ";
                        }
                        Console.Command(command);
                    }
                }
            }*/
            #endregion
        }
        
        private void mainListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            this.mainListView.DoDragDrop(e.Item, DragDropEffects.Copy);
        } 

        private void mainListView_DragLeave(object sender, DragEventArgs e)
        {
            //---------------- Inserted into DragDrop -------------- //

       /*     string command = "";
            string tmpPath = System.IO.Path.GetTempPath();
            string filePath;
            string[] tmpFilesPath = new string[selectedNames.Length];

            if (selectedNames.Length > 0)
            {
                for (int i = 0; i < selectedNames.Length; i++)
                {
                    tmpFilesPath[i] = tmpPath + "\\" + selectedNames[i];
                    filePath = selectedPaths[i];
                    command += "export " + filePath + " " + tmpPath + " ";
                    Console.Message("", "exporting " + selectedNames[i]);
                }
                Console.Command(command);
            }
            DataObject dataObject = new DataObject(DataFormats.FileDrop, tmpFilesPath);
            this.mainListView.DoDragDrop(dataObject, DragDropEffects.Copy);
            foreach (string fileName in selectedNames)
            {
                System.IO.File.Delete(tmpPath+"\\"+fileName);
            } */
        }

        //--------------- TreeView Drag&Drop -------------------//
        private void treeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            string command = "";
            if (e.Data.GetDataPresent("System.Windows.Forms.ListView+SelectedListViewItemCollection", false))
            {
                System.Drawing.Point pt = ( (TreeView) sender ).PointToClient(new System.Drawing.Point(e.X, e.Y));
                TreeNode dn = ( (TreeView) sender ).GetNodeAt(pt);
                ListView.SelectedListViewItemCollection lvi = (ListView.SelectedListViewItemCollection) e.Data.GetData("System.Windows.Forms.ListView+SelectedListViewItemCollection");

                foreach (ListViewItem item in lvi)
                {
                    command += "cp " + Address + "/" + item.Name + " " + "/" + dn.FullPath + " ";
                }
                Console.Command(command);
            }
        }

        //---------------Search---------------//

        private List<ListViewItem> searchHits;
        private List<Boolean> isDir;
        private int iterator;
        public void Search(String text)
        {
            searchHits = new List<ListViewItem>();
            isDir = new List<Boolean>();
            iterator = 0;
            #region Search with index
            String searchText = text ?? this.searchTextBox.Text;

            PVFS.Search.Index i = new PVFS.Search.Index();
            List<string> filePaths = i.Search(this.searchTextBox.Text);

            this.mainListView.Clear();
            int indexLastSlah;
            string fileName;
            foreach (String filePath in filePaths)
            {
                indexLastSlah = filePath.LastIndexOf('/');
                fileName = filePath.Substring(indexLastSlah + 1);
                //TODO: chech if it's a directory
                this.mainListView.Items.Add(fileName, fileName, 0);
            }
            /*this.mainListView.SelectedIndices.Clear();

            foreach (string filePath in filePaths)
            {
                if (mainListView.Items.ContainsKey(filePath))
                    mainListView.SelectedIndices.Add(mainListView.Items.IndexOfKey(filePath));
            }*/

            #endregion

            //----------TRY SEARCH WITH NODES

           var node = getNode(Address);
           SearchThroughDir(node, searchText);

           this.mainListView.Clear();

           foreach (var item in searchHits)
           {
               this.mainListView.Items.Add(item);
           }

            /*if (Address.Contains(searchText))
            {
                String path = Address.Substring(0, Address.IndexOf(searchText));
            } */
        }

        private void SearchThroughDir(TreeNode node, String text)
        {
            String itemName;
            foreach (ListViewItem item in this.mainListView.Items)
            {
                itemName = item.Name.Substring(item.Name.LastIndexOf('\\'));
                if (itemName.Contains(text))
                    searchHits.Add(item);
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                Console.Command("ls " + childNode.FullPath);
                SearchThroughDir(childNode, text);
            }
        }

        private void searchTextBox_Click(object sender, EventArgs e)
        {
            this.searchTextBox.SelectAll();
        }
        
        private void searchTextBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search();
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            var advSearchWindow = new AdvancedSearch();
            advSearchWindow.ShowDialog();
            Search
        }
    }
}
