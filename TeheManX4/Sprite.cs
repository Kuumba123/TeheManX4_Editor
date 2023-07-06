using System;
using System.Collections.Generic;

namespace TeheManX4
{
    class Sprite
    {
        #region Properties
        public List<Frame> frames = new List<Frame>();
        public List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
        public int cord;
        #endregion Properties

        #region Constructors
        internal Sprite()
        {
        }
        #endregion Constructors

        #region Methods
        internal byte[] CreateSprtData()
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
            int freeOffset = frames.Count * 4;

            foreach (var f in frames) //Loop for Setting up Quad Offsets
            {
                bw.Write((ushort)f.quads.Count);
                bw.Write((ushort)(freeOffset / 4));
                freeOffset += f.quads.Count * 4;
            }
            foreach (var f in frames) //Loop for Writting the Data
            {
                foreach (var q in f.quads)
                {
                    //Flags
                    byte flags = (byte)(Convert.ToByte(q.flipH) * 0x40);
                    flags += (byte)(Convert.ToByte(q.flipV) * 0x80);
                    flags += q.tpage;
                    flags += (byte)(q.clut << 2);

                    //Write Data
                    bw.Write(flags);
                    bw.Write(q.tex);
                    bw.Write(q.x);
                    bw.Write(q.y);
                }
            }
            return ms.ToArray();
        }
        internal static Frame GetFrame(byte[] data, int baseOffset, int frame = 0)
        {
            int offset = baseOffset;
            Frame f = new Frame();
            f.quads = new List<Quad>();
            ushort quadsAmount = BitConverter.ToUInt16(data, offset + frame * 4);
            if (quadsAmount > 1000 || quadsAmount < 1)
                throw new Exception("Exception: Invalid Quad Count");
            int quadsOffset = BitConverter.ToUInt16(data, offset + 2 + frame * 4) * 4;
            quadsOffset += baseOffset;
            for (int a = 0; a < quadsAmount; a++) //Loop through each Quad
            {
                Quad quad = new Quad();
                byte tmp = data[quadsOffset];

                quad.tpage = (byte)(tmp & 3);
                quad.clut = (byte)((tmp & 0xC) >> 2);
                quad.flipH = (tmp & 0x40) == 0x40;
                quad.flipV = (tmp & 0x80) == 0x80;
                quad.tex = data[quadsOffset + 1];
                quad.x = (sbyte)data[quadsOffset + 2];
                quad.y = (sbyte)data[quadsOffset + 3];

                //Add to quads List & Advance
                f.quads.Add(quad);
                quadsOffset += 4;
            }
            return f;
        }
        internal static List<Frame> GetFrames(int frameCount, byte[] data, int baseOffset)
        {
            List<Frame> frames = new List<Frame>();

            for (int i = 0; i < frameCount; i++)
            {
                Frame f = GetFrame(data, baseOffset, i);
                frames.Add(f);
            }
            return frames;
        }
        #endregion Methods
    }
    public class Quad
    {
        public byte tpage;
        public byte clut;
        public bool flipH;
        public bool flipV;
        public byte tex;
        public sbyte x;
        public sbyte y;
    }
    public class Frame
    {
        public List<Quad> quads;
    }
}
