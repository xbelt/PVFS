using System;
using System.Collections;
using System.IO;
using System.Text;
using VFS.VFS.Extensions;

namespace VFS.VFS.Models
{
    public sealed class VfsDisk : IDisposable{
        public String Password { get; set; }
        public FileStream Stream { get; private set; }
        public string Path { get; private set; }

        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;
        public BitArray BitMap { get; private set; }
        public DiskProperties DiskProperties { get; private set; }
        public VfsDirectory Root { get; private set; }

        //-----------Constructors and initialisation-----------

        public VfsDisk(string path, DiskProperties properties, string pw) {
            if (properties == null)
                throw new ArgumentNullException("properties");
            if (path == null)
                throw new ArgumentNullException("path");

            //find full path.
            path = (path.EndsWith("\\") ? path : path + "\\") + properties.Name + ".vdi";

            Stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            _writer = new BinaryWriter(Stream, new ASCIIEncoding(), false);
            _reader = new BinaryReader(Stream, new ASCIIEncoding(), false);
            Path = path;
            Password = pw;
            if (String.IsNullOrEmpty(pw))
            {
                Password = null;
            }
            DiskProperties = properties;
            BitMap = new BitArray(properties.NumberOfBlocks, true);
        }

        public void Init()
        {
            Root = EntryFactory.OpenEntry(this, DiskProperties.RootAddress, null) as VfsDirectory;
            InitBitArray();
        }

        private void InitBitArray()
        {
            _reader.Seek(this, 0, DiskProperties.BitMapOffset);
            var buffer = new byte[(int)Math.Ceiling(DiskProperties.NumberOfBlocks/8d)];
            _reader.Read(buffer, 0, buffer.Length);
            var temp = new BitArray(buffer);
            for (var i = 0; i < Math.Ceiling(DiskProperties.NumberOfBlocks / 8d); i++)
            {
                for (var j = 0; j < DiskProperties.NumberOfBlocks - 8 * i; j++)
                {
                    BitMap[8*i + j] = temp[8*i + (7 - j)];
                    if (j == 7)
                    {
                        break;
                    }
                }
            }
        }

        //-----------Access-----------

        /// <summary>
        /// Returns a binary reader for this disk (with arbitrary current position)
        /// </summary>
        public BinaryReader GetReader
        {
            get { return _reader; }
        }

        /// <summary>
        /// Returns a binary writer for this disk (with arbitrary current position)
        /// </summary>
        public BinaryWriter GetWriter
        {
            get { return _writer; }
        } 

        /// <summary>
        /// Indicates wether this disk has some free blocks.
        /// </summary>
        public bool IsFull() {
            return DiskProperties.NumberOfBlocks == DiskProperties.NumberOfUsedBlocks;
        }

        /// <summary>
        /// Returns the blocksize of this disk.
        /// </summary>
        public int BlockSize
        {
            get { return DiskProperties.BlockSize; }
        }

        //-----------Public interface-----------

        /// <summary>
        /// This method will seek the first free block and allocate it.
        /// </summary>
        /// <param name="address">the address of the allocated block</param>
        /// <returns>Returns a boolean indicating wether the operation was successfull</returns>
        public bool Allocate(out int address)
        {
            address = 0;
            for (var i = 0; i < BitMap.Length; i++)
            {
                if (!BitMap[i])
                {
                    address = i;
                    break;
                }
            }
            if (address == 0)
            {
                return false;
            }
            BitMap[address] = true;
            SetBit(true, address%8, DiskProperties.BitMapOffset + address/8, 0);
            DiskProperties.NumberOfUsedBlocks++;
            _writer.Seek(this, 0, DiskOffset.NumberOfUsedBlocks);
            _writer.Write(DiskProperties.NumberOfUsedBlocks);
            return true;
        }

        /// <summary>
        /// This method will allocate the specified number of blocks.
        /// </summary>
        /// <param name="address">the addresses of the allocated blocks</param>
        /// <param name="numberOfBlocks">The number of blocks that should be allocated.</param>
        /// <returns>Returns a boolean indicating wether the operation was successfull</returns>
        public bool Allocate(out int[] address, int numberOfBlocks)
        {
            address = new int[numberOfBlocks];
            var returnValue = true;
            for (var i = 0; i < numberOfBlocks; i++)
            {
                returnValue = Allocate(out address[i]) && returnValue;
            }
            return returnValue;
        }

        /// <summary>
        /// Frees the block with a specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        public void Free(int address) 
        {
            SetBit(false, address % 8, DiskProperties.BitMapOffset + address / 8, 0);
            BitMap[address] = false;
            DiskProperties.NumberOfUsedBlocks--;
            _writer.Seek(this, 0, DiskOffset.NumberOfUsedBlocks);
            _writer.Write(DiskProperties.NumberOfUsedBlocks);
        }
        
        /// <summary>
        /// Releases recources held by this Disk.
        /// </summary>
        public void Dispose()
        {
            Stream.Close(); // this closes reader&writer aswell.
            _reader.Close(); // But the stupid code analysis wants this too :(
            _writer.Close();
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
            _reader.Seek(this, address, byteIndex);

            var buffer = new byte[1];
            _reader.Read(buffer, 0, 1);

            var existingValue = buffer[0];
            if (value)
            {
                existingValue |= (byte) Math.Pow(2, 7 - bitIndex);
            }
            else
            {
                existingValue &= (byte)(255 - (byte) Math.Pow(2, 7 - bitIndex));
            }
            _writer.Seek(this, address, byteIndex);
            _writer.Write(existingValue);

        }

        //-----------Defragmentation helpers-----------

        /// <summary>
        /// Moves a block from the src to dst address.
        /// </summary>
        public void Move(int srcAddress, int dstAddress)
        {
            Free(srcAddress);
            _reader.Seek(this, srcAddress, FileOffset.ParentAddress);
            var parentId = _reader.ReadInt32();
            _reader.Seek(this, parentId, FileOffset.Header);
            var offset = FileOffset.Header / 4;
            while (_reader.ReadInt32() != srcAddress)
            {
                offset++;
                if (offset*4 > DiskProperties.BlockSize)
                {
                    _reader.Seek(this, parentId, FileOffset.NextBlock);
                    parentId = _reader.ReadInt32();
                    if (parentId == 0)
                    {
                        throw new DirectoryNotFoundException("parent did not contain desired file");
                    }
                    _reader.Seek(this, parentId, FileOffset.SmallHeader);
                    offset = FileOffset.SmallHeader/4;
                    break;
                }
            }
            _writer.Seek(this, parentId, offset*4);
            _writer.Write(dstAddress);

            var buffer = new byte[DiskProperties.BlockSize];
            _reader.Seek(this, srcAddress);
            _reader.Read(buffer, 0, DiskProperties.BlockSize);
            _writer.Seek(this, dstAddress);
            _writer.Write(buffer);
        }
    }
}
