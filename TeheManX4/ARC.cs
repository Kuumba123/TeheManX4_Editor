using System;
using System.Collections.Generic;
using System.Linq;

namespace TeheManX4
{
    public class ARC
    {
        #region Properties
        public List<Entry> entries = new List<Entry>();
        public string path;
        public string filename;
        public bool isMSB = false;
        #endregion Properties

        #region Constructors
        public ARC()
        {
        }
        public ARC(byte[] data)
        {
            if (data.Length < 0x800)
                return;
            int fileSize = BitConverter.ToInt32(data, 4);
            if (fileSize == BitConverter.ToInt32(BitConverter.GetBytes(data.Length).Reverse().ToArray(), 0))
            {
                fileSize = BitConverter.ToInt32(BitConverter.GetBytes(fileSize).Reverse().ToArray(), 0);
                isMSB = true;
            }
            else if (fileSize != data.Length)
            {
                return;
            }

            int type;
            int size;
            int o = 0x800;
            try
            {
                int fileCount = BitConverter.ToInt32(data, 0);
                if (isMSB)
                {
                    fileCount = BitConverter.ToInt32(BitConverter.GetBytes(fileCount).Reverse().ToArray(), 0);
                }
                for (int i = 0; i < fileCount; i++)
                {
                    if (i == 0)
                    {
                        type = BitConverter.ToInt32(data, 8);
                        size = BitConverter.ToInt32(data, 0xC);
                    }
                    else
                    {
                        type = BitConverter.ToInt32(data, (i - 1) * 8 + 0x10);
                        size = BitConverter.ToInt32(data, (i - 1) * 8 + 0x14);
                    }
                    if (isMSB)
                    {
                        type = BitConverter.ToInt32(BitConverter.GetBytes(type).Reverse().ToArray(), 0);
                        size = BitConverter.ToInt32(BitConverter.GetBytes(size).Reverse().ToArray(), 0);
                    }

                    //Add in new Entry
                    var e = new Entry();
                    e.type = type;
                    e.data = new byte[size];
                    Array.Copy(data, o, e.data, 0, size);
                    entries.Add(e);
                    //Sector Padding
                    if ((size % 0x800) != 0)
                        o += 0x800 - (size % 0x800) + size;
                    else
                        o += size;
                }
            }
            catch (Exception e)
            {
                //error = e.Message;
            }
        }
        #endregion Constructors

        #region Methods
        public byte[] LoadEntry(int id)
        {
            foreach (var e in entries)
            {
                if (e.type != id)
                    continue;
                return e.data;
            }
            return null;
        }
        public void LoadEntry(int id, byte[] dump)
        {
            for (int i = 0; i < this.entries.Count; i++)
            {
                if (entries[i].type != id)
                    continue;
                Array.Copy(entries[i].data, dump, entries[i].data.Length);
                return;
            }
        }
        public void SaveEntry(int id, byte[] dump)
        {
            for (int i = 0; i < this.entries.Count; i++)
            {
                if (entries[i].type != id)
                    continue;
                if (entries[i].data.Length != dump.Length)
                    Array.Resize(ref entries[i].data, dump.Length);
                Array.Copy(dump, entries[i].data, dump.Length);
                return;
            }
        }
        public void SaveTrimedEntry(int id, byte[] dump)
        {
            for (int i = 0; i < this.entries.Count; i++)
            {
                if (entries[i].type != id)
                    continue;
                Array.Copy(dump, entries[i].data, entries[i].data.Length);
                return;
            }
        }
        public byte[] GetEntriesData()  //With Filler + Header
        {
            int size = 0x800;
            //Calculate Size of byte Array
            foreach (var e in entries)
            {
                int s = e.data.Length;
                if (s % 0x800 != 0)
                {
                    s += 0x800 - (s % 0x800);
                }
                size += s;
            }
            byte[] data = new byte[size]; //+0x800 cus Header

            int addr_W = 0x800;
            foreach (var e in entries)
            {
                Array.Copy(e.data, 0, data, addr_W, e.data.Length);
                int s = e.data.Length;
                //Padding
                if (s % 0x800 != 0)
                {
                    s += 0x800 - (s % 0x800);
                }
                addr_W += s;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (i == 0)
                {
                    //Amount of Entries
                    if (!this.isMSB)
                        BitConverter.GetBytes(entries.Count).CopyTo(data, 0);
                    else
                        BitConverter.GetBytes(entries.Count).Reverse().ToArray().CopyTo(data, 0);
                    //File Size
                    if (!this.isMSB)
                        BitConverter.GetBytes(size).CopyTo(data, 4);
                    else
                        BitConverter.GetBytes(size).Reverse().ToArray().CopyTo(data, 4);
                    //Type
                    if (!this.isMSB)
                        BitConverter.GetBytes(entries[i].type).CopyTo(data, 8);
                    else
                        BitConverter.GetBytes(entries[i].type).Reverse().ToArray().CopyTo(data, 8);
                    //Size
                    if (!this.isMSB)
                        BitConverter.GetBytes(entries[i].data.Length).CopyTo(data, 0xC);
                    else
                        BitConverter.GetBytes(entries[i].data.Length).Reverse().ToArray().CopyTo(data, 0xC);
                    continue;
                }
                //Type
                if (!this.isMSB)
                    BitConverter.GetBytes(entries[i].type).CopyTo(data, ((i - 1) * 8) + 0x10);
                else
                    BitConverter.GetBytes(entries[i].type).Reverse().ToArray().CopyTo(data, ((i - 1) * 8) + 0x10);
                //Size
                if (!this.isMSB)
                    BitConverter.GetBytes(entries[i].data.Length).CopyTo(data, ((i - 1) * 8) + 0x14);
                else
                    BitConverter.GetBytes(entries[i].data.Length).Reverse().ToArray().CopyTo(data, ((i - 1) * 8) + 0x14);
            }
            return data;
        }
        public bool ContainsEntry(int type)
        {
            for (int i = 0; i < this.entries.Count; i++)
            {
                if (entries[i].type != type)
                    continue;
                return true;
            }
            return false;
        }
        public int GetIndexOfType(int type)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].type != type)
                    continue;
                return i;
            }
            return -1;
        }
        public int GetEntrySize(int type)
        {
            foreach (var e in entries)
            {
                if (e.type == type)
                    return e.data.Length;
            }
            return 0;
        }
        public int GetSize()
        {
            int size = 0;
            foreach (var e in entries)
            {
                size += e.data.Length;
            }
            return size;
        }
        #endregion Methods
    }
}
