using System.Collections.Generic;
using System.IO;

namespace TeheManX4
{
    class XA
    {
        #region Properties
        public List<byte[]> channels = new List<byte[]>();
        #endregion Properties

        #region Constructors
        public XA(byte[] data) //Read in XA File
        {
            byte[] buffer;
            BinaryReader br = new BinaryReader(new MemoryStream(data));
            BinaryWriter sw;
            for (int i = 0; i < 32; i++)
            {
                MemoryStream ms = new MemoryStream();
                sw = new BinaryWriter(ms);
                br.BaseStream.Position = 0;
                while (true)
                {
                    if (br.BaseStream.Position == data.Length)
                        break;

                    buffer = br.ReadBytes(0x920);

                    if (buffer[1] > 31) //Validation
                        return;
                    if (buffer[1] != i) //Channel Check
                        continue;
                    sw.Write(buffer);
                }
                if (ms.Length != 0)
                    channels.Add(ms.ToArray());
            }

        }
        #endregion Constructors

        #region Methods
        public void AssignChannel(byte index,byte id)
        {
            int offset = 1;
            while (true)
            {
                if (offset >= channels[index].Length)
                    return;
                channels[index][offset] = id;
                offset += 0x920;
            }
        }
        #endregion Methods
    }
}
