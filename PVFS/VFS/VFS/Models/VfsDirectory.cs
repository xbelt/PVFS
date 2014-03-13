using System;
using System.Collections.Generic;
using System.Linq;

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

    //TODO: GetFile-Functions could use NrOfChildren instead of Elements.Count
    public class VfsDirectory : VfsFile
    {
        public VfsDirectory(VfsDisk disk) : base(disk)
        {
        }

        public List<VfsFile> Elements { get; set; }

        public VfsDirectory GetSubDirectory(string name)
        {
            try
            {
                return Elements.Single(el => el.isDirectory) as VfsDirectory;
            }
            catch (Exception e)
            {
                throw new InvalidDirectoryException("directory " + name + " not found");
            }
        }

        public List<VfsFile> GetSubDirectories()
        {
            return Elements.Where(el => el.isDirectory) as List<VfsFile>;
        } 

        public List<VfsFile> GetFiles() {
            return Elements.Where(element => !element.isDirectory) as List<VfsFile>;
        }

        
        public VfsFile GetFileIgnoringSubDirectories(string name) {
            if (Elements.Count <= 0) {
                throw new Exception("Elements.Count was < 1");
            }
            if (name == null) {
                throw new Exception("Argument 'name' is null.");
            }

            return Elements.Where(element => !element.isDirectory).FirstOrDefault(element => element.Name.Equals(name));
        }

        public VfsFile GetFileCheckingSubDirectories ( string name ) {
            VfsFile result;
            if (name == null)
                throw new Exception("argument 'name' was null");
            if (Elements.Count <= 0)
                throw new Exception("Elements.Count was < 1");
            
            foreach (VfsFile element in Elements)
            {
                if (element.isDirectory)
                {
                    var dir = (VfsDirectory) element;
                    result = dir.GetFileCheckingSubDirectories(name);
                    if (result != null)
                        return result;
                }
                else
                {
                    if (element.Name.Equals(name))
                    {
                        return element;
                    }
                }
            }
            throw new Exception("File" + name + "was not found");
        }

        //TODO: might throw exception
        public void AddElement(VfsFile element) 
        {
            if (element != null) {
                if (Elements != null) {
                    Elements.Remove(element);
                } else {
                    throw new Exception("Elements was null");
                }
            } else {
                throw new Exception("argument 'element' was null");
            }
        }

        //TODO: might throw excpetion
        public void RemoveElement(VfsFile element)
        {
            if (element != null) {
                if (Elements != null) {
                    Elements.Remove(element);
                }
                else {
                    throw new Exception("Elements was null");
                }
            }
            else {
                throw new Exception("argument 'element' was null");
            }
        }
    }

    public class InvalidDirectoryException : Exception
    {
        public InvalidDirectoryException(string s)
        {
            throw new NotImplementedException();
        }
    }
}
