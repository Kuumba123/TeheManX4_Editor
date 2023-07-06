using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TeheManX4
{
    class Level
    {
        #region Properties
        public ARC arc;
        public byte[] layout;
        public byte[] screenData;
        public byte[] tileInfo;
        public int width = 0; //Width & Height of Layout
        public int height = 0;
        public int size = 0;
        public ARC clut_X;
        public ARC clut_Z;
        public bool megaEdit;
        public bool zeroEdit;
        public List<Enemy> enemies = new List<Enemy>();
        public List<Enemy> startEnemies = new List<Enemy>();
        public bool edit = false;
        public DateTime time; //Last File write time
        #endregion Properties

        #region Fields
        public static byte[] pixels = new byte[0x58000];
        public static byte[] tilebuffer = new byte[0x2000];
        public static int Id = 0;
        public static int BG = 0;
        public static bool zeroFlag = false;
        public static WriteableBitmap[] bmp = new WriteableBitmap[16];
        public static BitmapPalette[] palette = new BitmapPalette[0x80];
        public static ARC[] playerArcs = new ARC[3];
        #endregion Fields

        #region Constructors
        public Level()
        {
        }
        #endregion Constructors

        #region Methods
        public static void LoadLevels(string path) //And Player Files
        {
            PSX.levels.Clear();
            for (int i = 0; i < 0xD; i++)
            {
                //1ST HALF
                if (File.Exists(path + "/ARC/" + "ST" + Convert.ToString(i, 16).PadLeft(2, '0') + "_00.ARC") && i != 0xB)
                {
                    int o = PSX.levels.Count;
                    PSX.levels.Add(new Level());
                    PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/" + "ST" + Convert.ToString(i, 16).PadLeft(2, '0') + "_00.ARC"));
                    PSX.levels[o].arc.filename = "ST" + Convert.ToString(i, 16).PadLeft(2, '0').ToUpper() + "_00.ARC";
                    PSX.levels[o].arc.path = path;
                    PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/" + "ST" + Convert.ToString(i, 16).PadLeft(2, '0') + "_00.ARC");

                    if (File.Exists(path + "/ARC/" + "COL0" + Convert.ToString(i, 16) + "_0X.ARC"))
                    {
                        PSX.levels[o].clut_X = new ARC(File.ReadAllBytes(path + "/ARC/" + "COL0" + Convert.ToString(i, 16) + "_0X.ARC"));
                        PSX.levels[o].clut_X.filename = "COL0" + Convert.ToString(i, 16).ToUpper() + "_0X.ARC";
                    }

                    if (File.Exists(path + "/ARC/" + "COL0" + Convert.ToString(i, 16) + "_0Z.ARC"))
                    {
                        PSX.levels[o].clut_Z = new ARC(File.ReadAllBytes(path + "/ARC/" + "COL0" + Convert.ToString(i, 16) + "_0Z.ARC"));
                        PSX.levels[o].clut_Z.filename = "COL0" + Convert.ToString(i, 16).ToUpper() + "_0Z.ARC";
                    }
                }
                if(i == 0xB)
                {
                    if(File.Exists(path + "/ARC/ST0B_0X.ARC"))
                    {
                        int o = PSX.levels.Count;
                        PSX.levels.Add(new Level());
                        PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/ST0B_0X.ARC"));
                        PSX.levels[o].arc.filename = "ST0B_0X.ARC";
                        PSX.levels[o].arc.path = path;
                        PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/ST0B_0X.ARC");

                        if (File.Exists(path + "/ARC/COL0B_0X.ARC"))
                        {
                            PSX.levels[o].clut_X = new ARC(File.ReadAllBytes(path + "/ARC/COL0B_0X.ARC"));
                            PSX.levels[o].clut_X.filename = "COL0B_0X.ARC";
                        }
                    }
                    if (File.Exists(path + "/ARC/ST0B_0Z.ARC"))
                    {
                        int o = PSX.levels.Count;
                        PSX.levels.Add(new Level());
                        PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/ST0B_0Z.ARC"));
                        PSX.levels[o].arc.filename = "ST0B_0Z.ARC";
                        PSX.levels[o].arc.path = path;
                        PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/ST0B_0Z.ARC");

                        if (File.Exists(path + "/ARC/COL0B_0Z.ARC"))
                        {
                            PSX.levels[o].clut_Z = new ARC(File.ReadAllBytes(path + "/ARC/COL0B_0Z.ARC"));
                            PSX.levels[o].clut_Z.filename = "COL0B_0Z.ARC";
                        }
                    }
                }
                //MID
                if (File.Exists(path + "/ARC/" + "ST" + Convert.ToString(i, 16).PadLeft(2, '0') + "_01.ARC"))
                {
                    int o = PSX.levels.Count;
                    PSX.levels.Add(new Level());
                    PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/" + "ST" + Convert.ToString(i, 16).PadLeft(2, '0') + "_01.ARC"));
                    PSX.levels[o].arc.filename = "ST" + Convert.ToString(i, 16).PadLeft(2, '0').ToUpper() + "_01.ARC";
                    PSX.levels[o].arc.path = path;
                    PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/" + "ST" + Convert.ToString(i, 16).PadLeft(2, '0') + "_01.ARC");

                    if (File.Exists(path + "/ARC/" + "COL0" + Convert.ToString(i, 16) + "_1X.ARC"))
                    {
                        PSX.levels[o].clut_X = new ARC(File.ReadAllBytes(path + "/ARC/" + "COL0" + Convert.ToString(i, 16) + "_1X.ARC"));
                        PSX.levels[o].clut_X.filename = "COL0" + Convert.ToString(i, 16).ToUpper() + "_1X.ARC";
                    }

                    if (File.Exists(path + "/ARC/" + "COL0" + Convert.ToString(i, 16) + "_1Z.ARC"))
                    {
                        PSX.levels[o].clut_Z = new ARC(File.ReadAllBytes(path + "/ARC/" + "COL0" + Convert.ToString(i, 16) + "_1Z.ARC"));
                        PSX.levels[o].clut_Z.filename = "COL0" + Convert.ToString(i, 16).ToUpper() + "_1Z.ARC";
                    }
                }
            }
            if(File.Exists(path + "/ARC/ST0D_0X.ARC")) //StageSelect X
            {
                int o = PSX.levels.Count;
                PSX.levels.Add(new Level());
                PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/ST0D_0X.ARC"));
                PSX.levels[o].arc.filename = "ST0D_0X.ARC";
                PSX.levels[o].arc.path = path;
                PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/ST0D_0X.ARC");

                if(File.Exists(path + "/ARC/COL0D_0X.ARC"))
                {
                    PSX.levels[o].clut_X = new ARC(File.ReadAllBytes(path + "/ARC/COL0D_0X.ARC"));
                    PSX.levels[o].clut_X.filename = "COL0D_0X.ARC";
                }
            }
            if (File.Exists(path + "/ARC/ST0D_0Z.ARC")) //StageSelect Zero
            {
                int o = PSX.levels.Count;
                PSX.levels.Add(new Level());
                PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/ST0D_0Z.ARC"));
                PSX.levels[o].arc.filename = "ST0D_0Z.ARC";
                PSX.levels[o].arc.path = path;
                PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/ST0D_0Z.ARC");

                if (File.Exists(path + "/ARC/COL0D_0Z.ARC"))
                {
                    PSX.levels[o].clut_Z = new ARC(File.ReadAllBytes(path + "/ARC/COL0D_0Z.ARC"));
                    PSX.levels[o].clut_Z.filename = "COL0D_0Z.ARC";
                }
            }
            for (int i = 1; i < 9; i++) //Boss Introduction
            {
                int o = PSX.levels.Count;
                PSX.levels.Add(new Level());
                PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/STD_1_" + i.ToString() + "U.ARC"));
                PSX.levels[o].arc.filename = "STD_1_" + i.ToString() + "U.ARC";
                PSX.levels[o].arc.path = path;
                PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/STD_1_" + i.ToString() + "U.ARC");

                if (File.Exists(path + "/ARC/COLD_1U" + i.ToString() + ".ARC"))
                {
                    PSX.levels[o].clut_X = new ARC(File.ReadAllBytes(path + "/ARC/COLD_1U" + i.ToString() + ".ARC"));
                    PSX.levels[o].clut_X.filename = "COLD_1U" + Convert.ToString(i, 16).ToUpper() + ".ARC";
                }
            }

            for (int i = 0; i < 2; i++) //Title Screen & Player Select
            {
                if(File.Exists(path + "/ARC/ST0E_U" + i + ".ARC"))
                {
                    int o = PSX.levels.Count;
                    PSX.levels.Add(new Level());
                    PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/ST0E_U" + i + ".ARC"));
                    PSX.levels[o].arc.filename = "ST0E_U" + i + ".ARC";
                    PSX.levels[o].arc.path = path;
                    PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/ST0E_U" + i + ".ARC");
                    if (File.Exists(path + "/ARC/COL0E_U" + i + ".ARC"))
                    {
                        PSX.levels[o].clut_X = new ARC(File.ReadAllBytes(path + "/ARC/COL0E_U" + i + ".ARC"));
                        PSX.levels[o].clut_X.filename = "COL0E_U" + i + ".ARC";
                    }
                }
            }
            if (File.Exists(path + "/ARC/ST0F_UX.ARC")) //X Weapon Get
            {
                int o = PSX.levels.Count;
                PSX.levels.Add(new Level());
                PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/ST0F_UX.ARC"));
                PSX.levels[o].arc.filename = "ST0F_UX.ARC";
                PSX.levels[o].arc.path = path;
                PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/ST0F_UX.ARC");
                if (File.Exists(path + "/ARC/COL0F_U0.ARC"))
                {
                    PSX.levels[o].clut_X = new ARC(File.ReadAllBytes(path + "/ARC/COL0F_U0.ARC"));
                    PSX.levels[o].clut_X.filename = "COL0F_U0.ARC";
                }
            }
            if (File.Exists(path + "/ARC/ST0F_UZ.ARC")) //Zero Weapon Get
            {
                int o = PSX.levels.Count;
                PSX.levels.Add(new Level());
                PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/ST0F_UZ.ARC"));
                PSX.levels[o].arc.filename = "ST0F_UZ.ARC";
                PSX.levels[o].arc.path = path;
                PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/ST0F_UZ.ARC");
                if (File.Exists(path + "/ARC/COL0F_U0.ARC"))
                {
                    PSX.levels[o].clut_X = new ARC(File.ReadAllBytes(path + "/ARC/COL0F_U0.ARC"));
                    PSX.levels[o].clut_X.filename = "COL0F_U0.ARC";
                }
            }
            if (File.Exists(path + "/ARC/ST0F_U1.ARC")) //Credits
            {
                int o = PSX.levels.Count;
                PSX.levels.Add(new Level());
                PSX.levels[o].arc = new ARC(File.ReadAllBytes(path + "/ARC/ST0F_U1.ARC"));
                PSX.levels[o].arc.filename = "ST0F_U1.ARC";
                PSX.levels[o].arc.path = path;
                PSX.levels[o].time = File.GetLastWriteTime(path + "/ARC/ST0F_U1.ARC");
                if (File.Exists(path + "/ARC/COL0F_U1.ARC"))
                {
                    PSX.levels[o].clut_X = new ARC(File.ReadAllBytes(path + "/ARC/COL0F_U1.ARC"));
                    PSX.levels[o].clut_X.filename = "COL0F_U1.ARC";
                }
            }

            for (int i = 0; i < 3; i++) //Player Files
            {
                if (File.Exists(path + $"/ARC/PL0{i}_U.ARC"))
                    playerArcs[i] = new ARC(File.ReadAllBytes(path + $"/ARC/PL0{i}_U.ARC"));
            }

            foreach (var l in PSX.levels)
                l.ExtractLevelData();
        }
        public static unsafe void Draw16xTile(int id, int x, int y, int stride, IntPtr dest)
        {
            id &= 0x3FFF;
            byte* buffer = (byte*)dest;

            if (id == 0) // 0 = Empty Tile
            {
                for (int Y = 0; Y < 16; Y++)
                {
                    for (int X = 0; X < 16; X++)
                    {
                        int index = ((x + X) * 3) + (y + Y) * stride;
                        buffer[index] = 0;
                        buffer[index + 1] = 0;
                        buffer[index + 2] = 0;
                    }
                }
                return;
            }

            // Get Tile Info
            int cordX = (BitConverter.ToInt32(PSX.levels[Id].tileInfo, id * 4) >> 16) & 0xF;
            int cordY = (BitConverter.ToInt32(PSX.levels[Id].tileInfo, id * 4) >> 20) & 0xF;
            int page = (BitConverter.ToInt32(PSX.levels[Id].tileInfo, id * 4) >> 24) & 0x7;
            int clut = (BitConverter.ToInt32(PSX.levels[Id].tileInfo, id * 4) >> 8) & 0x3F;

            IntPtr bmpBackBuffer = bmp[page + 8].BackBuffer;
            int bmpStride = bmp[page].BackBufferStride;

            for (int row = 0; row < 16; row++)
            {
                int destIndex = (x * 3) + (y + row) * stride;
                int sourceIndex = (cordX * 8) + ((cordY * 16 + row) * bmpStride);

                for (int col = 0; col < 16; col++)
                {
                    byte pixel = *(byte*)(bmpBackBuffer + sourceIndex + (col / 2));

                    if((col & 1) == 1)
                        pixel &= 0xF;
                    else
                        pixel >>= 4;

                    buffer[destIndex++] = palette[clut + 64].Colors[pixel].R;
                    buffer[destIndex++] = palette[clut + 64].Colors[pixel].G;
                    buffer[destIndex++] = palette[clut + 64].Colors[pixel].B;
                }
            }
        }


        public static void DrawScreen(int s, int stride, IntPtr ptr)
        {
            int total = PSX.levels[Id].screenData.Length / 0x200;
            if (s > total - 1)
                s = 0;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Draw16xTile(BitConverter.ToUInt16(PSX.levels[Id].screenData, (x * 2) + (y * 0x20) + (0x200 * s)), x * 16, y * 16, stride, ptr);
                }
            }
        }
        public static void DrawScreen(int s,int drawX,int drawY, int stride, IntPtr ptr)
        {
            int total = PSX.levels[Id].screenData.Length / 0x200;
            if (s > total - 1)
                s = 0;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Draw16xTile(BitConverter.ToUInt16(PSX.levels[Id].screenData, (x * 2) + (y * 0x20) + (0x200 * s)), (x * 16) + drawX, (y * 16) + drawY, stride, ptr);
                }
            }
        }
        public void ExtractLevelData()
        {
            this.screenData = this.arc.LoadEntry(0);
            this.tileInfo = this.arc.LoadEntry(1);

            int i = this.GetIndex();
            if (i == -1)
                return;
            //Get Layout Info
            this.width = PSX.exe[Const.LayoutSizeOffset + i * 2];
            this.height = PSX.exe[Const.LayoutSizeOffset + i * 2 + 1];
            this.size = width * height;
            this.layout = new byte[this.width * this.height * 3];
            //Load Layout
            int offset = (int)PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.LayoutDataPointersOffset + i * 4));
            Array.Copy(PSX.exe, offset, this.layout, 0, this.width * this.height * 3);

            //Get Enemy Data
            if (i < 26)
            {
                uint p = BitConverter.ToUInt32(PSX.exe, Const.EnemyDataPointersOffset + i * 4);
                if (p != 0)
                    this.LoadEnemyData(PSX.CpuToOffset(p));
                p = BitConverter.ToUInt32(PSX.exe, Const.StartEnemyDataPointersOffset + i * 4);
                if (p != 0)
                    this.LoadEnemyData(PSX.CpuToOffset(p),true);
            }
        }
        private void ApplyLevelsToARC()
        {
            this.arc.SaveEntry(0, this.screenData);
            this.arc.SaveEntry(1, this.tileInfo);
        }
        public void LoadTextures(bool onlyObj = false)
        {
            if (!onlyObj)
            {
                //BG Textures
                Array.Clear(pixels, 0, pixels.Length);
                this.arc.LoadEntry(0x010000, pixels);
                ConvertBmp(pixels);
                for (int i = 0; i < 8; i++)
                    bmp[i + 8].WritePixels(new Int32Rect(0, 0, 256, 256), pixels, 128, i * 0x8000);
            }

            //Object Textures
            Array.Clear(pixels, 0, pixels.Length);
            if (this.arc.ContainsEntry(0x10102)) //Regualr Stage Objects
            {
                {   //Dump Object Textures
                    int addrW = 0;
                    int count = 0x5800;
                    byte[] data = this.arc.LoadEntry(0x10102);

                    for (int i = 0; i < data.Length; i++)
                    {
                        if (count == 0) //GOTO Y:0
                        {
                            addrW = i / 0x5800 * 0x8000;
                            count = 0x5800;
                        }
                        pixels[addrW] = data[i];
                        addrW++;
                        count--;
                    }
                }


                if (!zeroFlag) //X Player File
                {
                    if(playerArcs[0] != null && !MainWindow.settings.ultimate)
                    {
                        if (playerArcs[0].ContainsEntry(0x10206))
                            DumpPlayerTexture(playerArcs[0].LoadEntry(0x10206),pixels);
                    }else if(playerArcs[2] != null && MainWindow.settings.ultimate)
                    {
                        if (playerArcs[2].ContainsEntry(0x10206))
                            DumpPlayerTexture(playerArcs[2].LoadEntry(0x10206), pixels);
                    }
                }
                else //Zero Player File
                {
                    if (playerArcs[1] != null)
                    {
                        if (playerArcs[1].ContainsEntry(0x10206))
                            DumpPlayerTexture(playerArcs[1].LoadEntry(0x10206), pixels);
                    }
                }
            }
            else //For Title Screen
                this.arc.LoadEntry(0x10002, pixels);

            ConvertBmp(pixels);
            for (int i = 0; i < 8; i++)
                bmp[i].WritePixels(new Int32Rect(0, 0, 256, 256), pixels, 128, i * 0x8000);
        }
        private void DumpPlayerTexture(byte[] texData,byte[] dest)
        {
            int count = 80 * 128;
            int addrW = 176 * 128;
            for (int i = 0x2800; i < texData.Length; i++)
            {
                pixels[addrW] = texData[i];
                addrW++;
                count--;
                if (count == 0) //GOTO Y:0
                {
                    addrW = (i / 0x2800 * 0x8000) + 0x5800;
                    count = 80 * 128;
                }
            }
        }
        public byte[] GetSpawnData()
        {
            int index = this.GetIndex();
            byte[] data = new byte[36];

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            for (int i = 0; i < Settings.MaxPoints[index] + 1; i++)
            {
                uint addr = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CheckPointPointersAddress + index * 4)));
                uint read = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(addr + i * 4)));

                Array.Copy(PSX.exe, PSX.CpuToOffset(read), data, 0, 36);
                bw.Write(data);
            }
            return ms.ToArray();
        }
        private void LoadEnemyData(int p ,bool startEnemies = false)
        {
            try
            {
                while (true)
                {
                    if (PSX.exe[p + 3] == 0xFF)
                        return;
                    //Add New Enemy
                    var e = new Enemy();
                    e.range = (byte)((PSX.exe[p] >> 4 ) & 0xF);
                    e.id = PSX.exe[p + 1];
                    e.var = PSX.exe[p + 2];
                    e.type = PSX.exe[p + 3];
                    e.x = BitConverter.ToInt16(PSX.exe, p + 4);
                    e.y = BitConverter.ToInt16(PSX.exe, p + 6);
                    if (startEnemies)
                        this.startEnemies.Add(e);
                    else
                        this.enemies.Add(e);
                    p += 8;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Application.Current.Shutdown();
            }
        }
        public static byte[] CreateEnemyData(List<Enemy> enemies)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            foreach (var e in enemies)
            {
                bw.Write((byte)(e.range << 4));
                bw.Write(e.id);
                bw.Write(e.var);
                bw.Write(e.type);
                bw.Write(e.x);
                bw.Write(e.y);
            }
            //End entry
            bw.Write((ushort)0);
            bw.Write((byte)0);
            bw.Write((byte)0xFF);
            bw.Write((ushort)0);
            bw.Write((ushort)0);
            return ms.ToArray();
        }
        public static bool ValidLevel(int id)
        {
            int index = PSX.levels[id].GetIndex();
            int levelId = PSX.levels[id].GetId();
            if (!PSX.levels[id].isMid() && index < 26)
            {
                //Check if there is a valid number of enemies
                int enemiesCount;

                if (PSX.levels[id].GetId() != 9 && PSX.levels[id].GetId() != 0xA)
                    enemiesCount = PSX.levels[id].enemies.Count + PSX.levels[id].startEnemies.Count + PSX.levels[id + 1].enemies.Count + PSX.levels[id + 1].startEnemies.Count;
                else
                    enemiesCount = PSX.levels[id].enemies.Count;
                if(enemiesCount > Const.MaxEnemies[levelId])
                {
                    MessageBox.Show(PSX.levels[id].arc.filename + " has " + enemiesCount + " (including the mid stage) when the max is: " + Const.MaxEnemies[levelId] + ". The save/export was cancelled!");
                    return false;
                }
            }
            return true;
        }
        public static bool ValidLayouts()
        {
            int size = 0;
            int oldIndex = -1;

            foreach (var l in PSX.levels)
            {
                int newIndex = l.GetIndex();
                if (newIndex == oldIndex)
                    continue;
                oldIndex = newIndex;
                size += l.width * l.height * 3;
            }
            if(size > Const.MaxLayoutSize)
            {
                MessageBox.Show("The total size of all levels layouts in the game (in bytes) combined are: " + size + " , the max is " + Const.MaxLayoutSize);
                return false;
            }
            return true;
        }
        public static bool ValidPlayer()
        {
            int player = -1;
            int stageId = PSX.levels[Id].GetId();
            //Use Player File to Get Start Address
            if (!zeroFlag && playerArcs[0] != null && !MainWindow.settings.ultimate)
                player = 0;
            else if (!zeroFlag && playerArcs[2] != null )
                player = 2;
            else if (zeroFlag && playerArcs[1] != null)
                player = 1;

            if (player == -1 && stageId != 0xE)
            {
                MessageBox.Show("The Reload function needs the player file in order for the Reload function to work.");
                return false;
            }
            Settings.playerFileSize = 0;
            foreach (var e in playerArcs[player].entries)
            {
                if ((e.type >> 16) != 0)
                    continue;
                Settings.playerFileSize += e.data.Length;
            }
            if (stageId != 0xE)
                Settings.levelStartAddress = (uint)(Settings.playerFileSize + Const.ArcBufferAddress);
            else
                Settings.levelStartAddress = Const.ArcBufferAddress;

            Settings.levelSize = 0;
            foreach (var entry in PSX.levels[Id].arc.entries)
            {
                if ((entry.type >> 16) != 0)
                    continue;
                if (entry.type == 0)
                    Settings.levelScreenAddress = (uint)(Settings.levelSize + Settings.levelStartAddress);
                else if(entry.type == 1)
                    Settings.levelTileAddress = (uint)(Settings.levelSize + Settings.levelStartAddress);
                Settings.levelSize += entry.data.Length;
            }
            return true;
        }
        public static void SaveLevel(int id)
        {
            int index = PSX.levels[id].GetIndex();
            int levelId = PSX.levels[id].GetId();
            if(!PSX.levels[id].isMid() && index < 26) //Saving Enemy Data
            {
                //Regular Enemies
                int freeOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.EnemyDataPointersOffset + index * 4));
                byte[] data = CreateEnemyData(PSX.levels[id].enemies);
                data.CopyTo(PSX.exe, freeOffset);
                BitConverter.GetBytes(PSX.OffsetToCpu(freeOffset)).CopyTo(PSX.exe, Const.EnemyDataPointersOffset + index * 4);
                freeOffset += data.Length;

                //Start Enemies
                data = CreateEnemyData(PSX.levels[id].startEnemies);
                data.CopyTo(PSX.exe, freeOffset);
                BitConverter.GetBytes(PSX.OffsetToCpu(freeOffset)).CopyTo(PSX.exe, Const.StartEnemyDataPointersOffset + index * 4);

                //For Mid Stage
                if (levelId != 0x9 && levelId != 0xA)
                {
                    int offset = 1;
                    if (PSX.levels[id].arc.filename == "ST0B_0X.ARC")
                        offset = 2;

                    //Regular Enemies
                    freeOffset += data.Length;
                    data = CreateEnemyData(PSX.levels[id + offset].enemies);
                    data.CopyTo(PSX.exe, freeOffset);
                    BitConverter.GetBytes(PSX.OffsetToCpu(freeOffset)).CopyTo(PSX.exe, Const.EnemyDataPointersOffset + (index + offset) * 4);
                    freeOffset += data.Length;
                    //Start Enemies
                    data = CreateEnemyData(PSX.levels[id + offset].startEnemies);
                    data.CopyTo(PSX.exe, freeOffset);
                    BitConverter.GetBytes(PSX.OffsetToCpu(freeOffset)).CopyTo(PSX.exe, Const.StartEnemyDataPointersOffset + (index + offset) * 4);
                }
            }
            PSX.levels[id].ApplyLevelsToARC();
        }
        public static void SaveLayouts()
        {

            int freeOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.LayoutDataPointersOffset));
            int oldIndex = -1;
            foreach (var l in PSX.levels)
            {
                int index = l.GetIndex();

                if (index == oldIndex)
                    continue;
                oldIndex = index;

                l.layout.CopyTo(PSX.exe, freeOffset);
                BitConverter.GetBytes(PSX.OffsetToCpu(freeOffset)).CopyTo(PSX.exe, Const.LayoutDataPointersOffset + index * 4);
                freeOffset += l.layout.Length;
                PSX.exe[Const.LayoutSizeOffset + index * 2] = (byte)l.width;
                PSX.exe[Const.LayoutSizeOffset + 1 + index * 2] = (byte)l.height;
            }
        }
        public int GetIndex()
        {
            if (!arc.filename.Contains("ST"))
                return -1;
            if (arc.filename.Contains("STD_1"))
                return 0xD * 2 + 1;
            int mid = 0;
            if (arc.filename.Contains("_01.ARC") || arc.filename.Contains("U1"))
                mid = 1;
            return Convert.ToInt32(arc.filename[3].ToString(), 16) * 2 + mid;
        }
        public int GetId()
        {
            if (!arc.filename.Contains("ST"))
                return -1;
            if (arc.filename.Contains("STD_1"))
                return 0xD;
            return Convert.ToInt32(arc.filename[3].ToString(), 16);
        }
        public bool isMid()
        {
            if (!arc.filename.Contains("ST"))
                return false;
            if (arc.filename.Contains("STD_1"))
                return true;
            if (this.arc.filename.Contains("_01.ARC") || this.arc.filename.Contains("U1"))
                return true;
            return false;
        }
        public static void AssignPallete()
        {
            bool noPal = false;
            if (!zeroFlag && PSX.levels[Id].clut_X == null)
                noPal = true;
            if (zeroFlag && PSX.levels[Id].clut_Z == null)
                noPal = true;
            if (noPal)
            {
                for (int i = 0; i < palette.Length; i++)
                    palette[i] = Const.GreyScalePallete;
            }
            for (int b = 0; b < 0x80; b++)
            {
                List<Color> l = new List<Color>();
                for (int i = 0; i < 16; i++)
                {
                    ushort color;
                    if (!zeroFlag && PSX.levels[Id].clut_X != null)
                        color = BitConverter.ToUInt16(PSX.levels[Id].clut_X.entries[0].data, (i + b * 16) * 2);
                    else if (zeroFlag && PSX.levels[Id].clut_Z != null)
                        color = BitConverter.ToUInt16(PSX.levels[Id].clut_Z.entries[0].data, (i + b * 16) * 2);
                    else
                        goto End;

                    byte R = (byte)(color % 32 * 8);
                    byte G = (byte)(color / 32 % 32 * 8);
                    byte B = (byte)(color / 1024 % 32 * 8);
                    l.Add(Color.FromRgb(R, G, B));
                }
                palette[b] = new BitmapPalette(l);
            }
        End:
            for (int a = 0; a < bmp.Length; a++)
            {
                if (bmp[a] == null)
                    bmp[a] = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Indexed4, Const.GreyScalePallete);
            }
        }
        public static void AssignPallete(int clut)
        {
            List<Color> l = new List<Color>();
            for (int i = 0; i < 16; i++)
            {
                ushort color;
                if (!zeroFlag)
                    color = BitConverter.ToUInt16(PSX.levels[Id].clut_X.entries[0].data, (i + clut * 16) * 2);
                else
                    color = BitConverter.ToUInt16(PSX.levels[Id].clut_Z.entries[0].data, (i + clut * 16) * 2);

                byte R = (byte)(color % 32 * 8);
                byte G = (byte)(color / 32 % 32 * 8);
                byte B = (byte)(color / 1024 % 32 * 8);
                l.Add(Color.FromRgb(R, G, B));
            }
            palette[clut] = new BitmapPalette(l);
        }
        public static int GetClut(int id)
        {
            return (BitConverter.ToInt32(PSX.levels[Id].tileInfo, id * 4) >> 16) & 0x3F;
        }
        public static int To32Rgb(int color)
        {
            byte R = (byte)(color % 32 * 8);
            byte G = (byte)(color / 32 % 32 * 8);
            byte B = (byte)(color / 1024 % 32 * 8);
            return (R << 16) + (G << 8) + B;
        }
        public static int To15Rgb(byte B, byte G, byte R)
        {
            return B / 8 * 1024 + G / 8 * 32 + R / 8;
        }
        public static int GetSelectedTile(int c, double w, int d)
        {
            int i = (int)w;
            int e = i / d;
            return c / e;
        }
        public static void ConvertBmp(byte[] b)
        {
            //PSX 4bpp => BMP 4bpp
            int lc = 0;
            while (lc != b.Length)
            {
                var n1 = (b[lc] & 0xF) << 4;
                var n2 = (b[lc] >> 4) + n1;
                b[lc] = (byte)n2;
                lc++;
            }
        }
        public static void DecompressTexture(byte[] src, byte[] dest, int start)
        {
            int offset = start;
            int destOffset = 0;
        Start:
            ushort crtl = BitConverter.ToUInt16(src, offset);
            uint baseCrtl = 0x8000;
            int loopC = 0x10;
            offset += 2;
            while (true)
            {
                if ((baseCrtl & crtl) == 0)
                {
                    Array.Copy(src, offset, dest, destOffset, 2);
                    offset += 2;
                    destOffset += 2;
                }
                else
                {
                    ushort data = BitConverter.ToUInt16(src, offset);
                    ushort data2 = data;
                    offset += 2;
                    if ((data & 0xF800) == 0)
                    {
                        data = BitConverter.ToUInt16(src, offset);
                        offset += 2;
                    }
                    else
                    {
                        data >>= 0xB;
                        data2 &= 0x7FF;
                    }
                    if ((data | data2) == 0) //End
                        return;

                    if (data2 == 0) //if offset is 0 , copy 0s
                    {
                        while (true)
                        {
                            Array.Clear(dest, destOffset, 2);
                            data--;
                            destOffset += 2;
                            if (data == 0)
                                break;
                        }
                    }
                    else
                    {
                        int copyOffset = destOffset - (data2 * 2);
                        while (true)
                        {
                            Array.Copy(dest, copyOffset, dest, destOffset, 2);
                            copyOffset += 2;
                            destOffset += 2;
                            data--;
                            if (data == 0)
                                break;
                        }
                    }
                }
                loopC -= 1;
                baseCrtl >>= 1;
                if (loopC == 0)
                    goto Start;
            }
        }
        #endregion Methods
    }
}
