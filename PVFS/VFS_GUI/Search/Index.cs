using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using VFS.VFS.Models;

namespace PVFS.Search
{
    [Serializable]
    class Index
    {
        private int nextIndex;
        private readonly Dictionary<int, string> files;
        private readonly Dictionary<string, List<int>> wordIndex;

        public Index()
        {
            this.nextIndex = 0;
            this.files = new Dictionary<int, string>();
            this.wordIndex = new Dictionary<string, List<int>>();
        }

        public void add(string path)
        {
            if (!files.ContainsValue(path)) // Inefficient, use a reverse hashset to make more efficient.
            {
                files.Add(this.nextIndex, path);

                foreach (string s in new String[0])
                {
                    if (wordIndex.ContainsKey(s))
                    {
                        wordIndex[s].Add(this.nextIndex);
                    }
                    else
                    {
                        wordIndex.Add(s, new List<int> { this.nextIndex });
                    }
                }

                this.nextIndex++;
            }
            else
                throw new UnusualException("File was already indexed earlier");
        }

        /// <summary>
        /// Looks through the Index and returns a list of paths to files containing the keywords.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns a list of paths to files containing the keywords.</returns>
        public List<string> Search(string name)
        {
            return wordIndex[name].Select(file => this.files[file]).ToList();
        }

        /// <summary>
        /// Dont forget to Close() after use!
        /// </summary>
        static BinaryReader save(Index index)
        {
            Stream r = new MemoryStream();
            BinaryFormatter BF = new BinaryFormatter();
            BF.Serialize(r, index);
            return new BinaryReader(r);
        }

        static Index load(VfsFile file)
        {
            Stream stream = new MemoryStream();
            file.Read(new BinaryWriter(stream));
            BinaryFormatter BF = new BinaryFormatter();
            Index result = (Index)BF.Deserialize(stream);
            stream.Close();
            return result;
        }



    }

    internal class UnusualException : Exception
    {
        public UnusualException(string message) : base(message) { }

    }
}
