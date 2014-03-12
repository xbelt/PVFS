using System.Collections.Generic;

namespace VFS.VFS.Models
{
    /*
 * NextBlock 4
 * StartBlock 4
 * NrOfChildren 4
 * NoBlocks 4
 * Directory 1
 * NameSize 1
 * Name 110
 * Children x * 4
 */
    class VfsDirectory : VfsFile
    {
        public List<Block> Children { get; set; }

        public VfsEntry GetElements() {
            
        }

        public List<VfsFile> GetFiles() {
            
        }

        public VfsFile GetFile(string name) {
            
        }

        public void AddElement(VfsEntry element) {
            
        }

        public void RemoveElement(VfsEntry element) {
            
        }


    }
}
