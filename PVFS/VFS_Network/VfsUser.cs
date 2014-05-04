using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFS_Network
{
    [Serializable]
    public class VfsUser
    {
        public string Name { get; set; }

        public Byte[] PasswordHash { get; set; }

        public bool Online { get; set; }
    }
}
