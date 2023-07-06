using System.Collections.Generic;
using System.IO;

namespace TeheManX4
{
    class TextEntry
    {
        #region Properties
        public bool boxHigh;
        public List<Box> boxes = new List<Box>();
        #endregion Properties

        #region Methods
        internal static List<Box> GetBoxes(byte[] text,int offset)
        {
            List<Box> boxes = new List<Box>();
            MemoryStream readStream = new MemoryStream(text);
            BinaryReader br = new BinaryReader(readStream);
            br.BaseStream.Position = offset;
            while (true)
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                Box box = new Box();

                bool end = false;

                while (true)
                {
                    ushort data = br.ReadUInt16();
                    bw.Write(data);
                    if ((data & 0x8000) == 0x8000) //End Text
                    {
                        end = true;
                        break;
                    }
                    else if((data & 0x2000) == 0x2000) //End Box
                    {
                        break;
                    }
                }
                box.data = ms.ToArray();
                boxes.Add(box);

                if (end)
                    break;
            }
            return boxes;
        }
        internal byte[] CreateTextData()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bool check = false;
            foreach (var b in boxes)
            {
                MemoryStream readS = new MemoryStream(b.data);
                BinaryReader br = new BinaryReader(readS);
                while (true)
                {
                    ushort data = br.ReadUInt16();

                    if (!check)
                    {
                        if (boxHigh)
                            data |= 0x800;
                    }

                    bw.Write(data);

                    if ((data & 0x8000) == 0x8000) //End
                        break;
                    else if ((data & 0x2000) == 0x2000) //End of specfic Box
                        break;
                }
            }

            return ms.ToArray();
        }
        #endregion Methods
    }
    class Box
    {
        internal byte[] data;
    }
}
