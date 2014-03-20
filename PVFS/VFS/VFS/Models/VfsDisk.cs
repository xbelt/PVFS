using System;
using System.Collections;
using System.IO;
using System.Text;
using VFS.VFS.Extensions;

namespace VFS.VFS.Models
{
    public class VfsDisk {

        public VfsDisk(string path, DiskProperties properties) {
            FileStream stream;
            if (path.EndsWith("\\"))
            {
                stream = File.Open(path + properties.Name + ".vdi", FileMode.Create, FileAccess.ReadWrite);
            }
            else
            {
                stream = File.Open(path + "\\" + properties.Name + ".vdi", FileMode.Create, FileAccess.ReadWrite);
            }

            Writer = new BinaryWriter(stream, new ASCIIEncoding(), false);
            Reader = new BinaryReader(stream, new ASCIIEncoding(), false);
            Path = path;
            DiskProperties = properties;
            bitMap = new BitArray(properties.NumberOfBlocks, true);
        }

        public void Init()
        {
            root = EntryFactory.OpenEntry(this, DiskProperties.RootAddress, null) as VfsDirectory;
            InitBitArray();
        }

        public void InitBitArray()
        {
            Reader.Seek(this, 0, DiskProperties.BitMapOffset);
            var buffer = new byte[(int)Math.Ceiling(DiskProperties.NumberOfBlocks/8d)];
            Reader.Read(buffer, 0, buffer.Length);
            var temp = new BitArray(buffer);
            for (var i = 0; i < Math.Ceiling(DiskProperties.NumberOfBlocks / 8d); i++)
            {
                for (var j = 0; j < DiskProperties.NumberOfBlocks - 8 * i; j++)
                {
                    bitMap[8*i + j] = temp[8*i + (7 - j)];
                    if (j == 7)
                    {
                        break;
                    }
                }
            }
        }

        public BinaryReader Reader;
        public BinaryWriter Writer;
        public BitArray bitMap;
        public DiskProperties DiskProperties { get; set; }
        private string Path { get; set; }

        public BinaryReader getReader() 
        {
            return Reader;
        }
        public BinaryWriter getWriter()
        {
            return Writer;
        } 

        public bool isFull() {
            return DiskProperties.NumberOfBlocks == DiskProperties.NumberOfUsedBlocks;
        }

        public VfsDirectory root;
        #region Block
        public bool allocate(out int address)
        {
            address = 0;
            for (var i = 0; i < bitMap.Length; i++)
            {
                if (!bitMap[i])
                {
                    address = i;
                    break;
                }
            }
            if (address == 0)
            {
                return false;
            }
            SetBit(true, address%8, DiskProperties.BitMapOffset + address/8, 0);
            return true;
        }

        /// <summary>
        /// This method sets a bit in a byte to a certain value
        /// </summary>
        /// <param name="value">the value to be set (0 for false and 1 for true)</param>
        /// <param name="bitIndex">the index of the bit to be set. (where index 0 corresponds to the MSB)</param>
        /// <param name="byteIndex">the offset of the target byte regarding the address</param>
        /// <param name="address">the address as a base for the offset</param>
        private void SetBit(bool value, int bitIndex, int byteIndex, int address)
        {
            Reader.Seek(this, address, byteIndex);
            Writer.Seek(this, address, byteIndex);

            var buffer = new byte[1];
            Reader.Read(buffer, 0, 1);

            var existingValue = buffer[0];
            if (value)
            {
                existingValue |= (byte) Math.Pow(2, 7 - bitIndex);
            }
            else
            {
                existingValue &= (byte)(255 - (byte) Math.Pow(2, 7 - bitIndex));
            }
            Writer.Write(existingValue);

        }

        public bool allocate(out int[] address, int numberOfBlocks)
        {
            address = new int[numberOfBlocks];
            var returnValue = true;
            for (var i = 0; i < numberOfBlocks; i++)
            {
                returnValue = returnValue && allocate(out address[i]);
            }
            return returnValue;
        }

        public void free(int address) 
        {
            SetBit(false, address % 8, DiskProperties.BitMapOffset + address / 8, 0);
        }
        #endregion



        public int BlockSize
        {
            get { return DiskProperties.BlockSize; }
        }
    }
}
