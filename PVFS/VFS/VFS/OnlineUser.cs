using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VFS.VFS
{
    public class OnlineUser
    {
        private static OnlineUser empty = new OnlineUser() { Name = "" };
        public static OnlineUser Empty { get { return empty; } }

        public TcpClient Connection { get; set; }

        public string Name { get; set; }
    }
}
