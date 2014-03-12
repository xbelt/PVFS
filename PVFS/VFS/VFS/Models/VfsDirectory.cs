﻿using System.Collections.Generic;

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
    class VfsDirectory : VfsFile
    {
        //Changed from List<Blocks>
        public List<VfsEntry> Elements { get; set; }

        public List<VfsFile> GetFiles() {
            List<VfsFile> result = new List<VfsFile>();
            if (Elements.Count > 0) {
                foreach (VfsEntry element in Elements)
                {
                    if (!element.isDirectory) {
                        result.Add((VfsFile) element);
                    }
                }
            }
            return result;
        }

        
        public VfsFile GetFileIgnoringSubDirectories(string name) {
            if (Elements.Count > 0) {
                foreach (VfsEntry element in Elements) {
                    if (!element.isDirectory) {
                        VfsFile tmp = (VfsFile) element;
                        if (tmp.Name.Equals(name)) {
                            return (VfsFile) element;
                        }
                    }
                }
            }
            //Size was 0 or no file has been found
            //TODO: how to handle this exception?
            return null;
        }

        public VfsFile GetFileCheckingSubDirectories ( string name ) {
            VfsFile result;
            if (Elements.Count > 0) {
                foreach (VfsEntry element in Elements) {
                    if (element.isDirectory) {
                        VfsDirectory dir = (VfsDirectory) element;
                        result = dir.GetFileCheckingSubDirectories(name);
                        if (result != null)
                            return result;
                    } else {
                        VfsFile tmp = (VfsFile) element;
                        if (tmp.Name.Equals(name)) {
                            result = (VfsFile) element;
                            return result;
                        }
                    }
                }
            }
            //Size was 0 or no file has been found
            //TODO: how to handle this exception?
            return null;
        }

        //TODO: might throw exception
        public void AddElement(VfsEntry element) {
            Elements.Add(element);
        }

        //TODO: might throw excpetion
        public void RemoveElement(VfsEntry element) {
            Elements.Remove(element);
        }
    }
}
