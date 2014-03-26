﻿using System;
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
                stream = File.Open(path + properties.Name + ".vdi", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            else
            {
                stream = File.Open(path + "\\" + properties.Name + ".vdi", FileMode.OpenOrCreate, FileAccess.ReadWrite);
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
        /// <summary>
        /// This method will seek the first free block and allocate it.
        /// </summary>
        /// <param name="address">the address of the allocated block</param>
        /// <returns></returns>
        public bool Allocate(out int address)
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
            bitMap[address] = true;
            SetBit(true, address%8, DiskProperties.BitMapOffset + address/8, 0);
            DiskProperties.NumberOfUsedBlocks++;
            Writer.Seek(this, 0, DiskOffset.NumberOfUsedBlocks);
            Writer.Write(DiskProperties.NumberOfUsedBlocks);
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

        public bool Allocate(out int[] address, int numberOfBlocks)
        {
            address = new int[numberOfBlocks];
            var returnValue = true;
            for (var i = 0; i < numberOfBlocks; i++)
            {
                returnValue = returnValue && Allocate(out address[i]);
            }
            return returnValue;
        }

        public void Free(int address) 
        {
            SetBit(false, address % 8, DiskProperties.BitMapOffset + address / 8, 0);
            DiskProperties.NumberOfUsedBlocks--;
            Writer.Seek(this, 0, DiskOffset.NumberOfUsedBlocks);
            Writer.Write(DiskProperties.NumberOfUsedBlocks);
        }

        public void Move(int srcAddress, int dstAddress)
        {
            Reader.Seek(this, srcAddress, FileOffset.ParentAddress);
            var parentId = Reader.ReadInt32();
            Reader.Seek(this, parentId, FileOffset.Header);
            var offset = FileOffset.Header / 4;
            while (Reader.ReadInt32() != srcAddress)
            {
                offset++;
                if (offset*4 > DiskProperties.BlockSize)
                {
                    break;
                    //TODO: implement block switch
                }
            }
            Writer.Seek(this, parentId, offset*4);
            Writer.Write(dstAddress);

            var buffer = new byte[DiskProperties.BlockSize];
            Reader.Seek(this, srcAddress);
            Reader.Read(buffer, 0, DiskProperties.BlockSize);
            Writer.Seek(this, dstAddress);
            Writer.Write(buffer);
        }
        #endregion



        public int BlockSize
        {
            get { return DiskProperties.BlockSize; }
        }
    }
}
