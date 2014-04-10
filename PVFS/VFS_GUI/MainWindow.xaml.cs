using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VFS.VFS;

namespace VFS_GUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public LocalConsole Console { get; set; }

        public string Address { get; private set; }
    }

        public MainWindow()
        {
            InitializeComponent();

            this.console = new LocalConsole(this);
            VfsManager.Console = console;
        }
    }
}
