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
        public byte[] clutAnime;
        public bool megaEdit;
        public bool zeroEdit;
        public List<Enemy> enemies = new List<Enemy>();
        public List<Enemy> startEnemies = new List<Enemy>();
        public bool edit = false;
        public DateTime time; //Last File write time
        #endregion Properties

        #region Fields
        public static byte[] pixels = new byte[0x40000];
        public static int Id = 0;
        public static int BG = 0;
        public static bool showCollision;
        public static bool enemyExpand;
        public static bool textureSupport; //8bpp Textures
        public static WriteableBitmap collisionBmp;
        public static bool zeroFlag = false;
        public static WriteableBitmap[] bmp = new WriteableBitmap[16 + 4];
        public static BitmapPalette[] palette = new BitmapPalette[512];
        public static ARC[] playerArcs = new ARC[3];
        #endregion Fields

        #region Methods
        public static void LoadLevels(string path) //And Player Files
        {
            PSX.levels.Clear();

            if (PSX.exe[PSX.CpuToOffset(0x80028db4)] == 0x80)
                enemyExpand = true;
            else
                enemyExpand = false;

            /*8bpp Texture Support*/
            Visibility visibility;
            int max;
            string Texture8bpp = "TEXTURE_8BPP";
            if (System.Text.Encoding.ASCII.GetString(PSX.exe, PSX.CpuToOffset(0x800260e4), Texture8bpp.Length) == Texture8bpp)
            {
                visibility = Visibility.Visible;
                max = 0xB;
                textureSupport = true;
            }
            else
            {
                visibility = Visibility.Collapsed;
                max = 7;
                textureSupport = false;
            }
            MainWindow.window.screenE.pageInt.Maximum = max;
            MainWindow.window.x16E.pageInt.Maximum = max;
            Forms.ClutEditor.maxPage = max;

            for (int i = 0; i < 4; i++)
            {
                ((System.Windows.Controls.Button)MainWindow.window.x16E.tPagePannel.Children[9 + i]).Visibility = visibility;
                ((System.Windows.Controls.Button)MainWindow.window.clutE.pagePannel.Children[9 + i]).Visibility = visibility;
            }

            /*Actually Loading Levels*/
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
                if (!File.Exists(path + "/ARC/STD_1_" + i.ToString() + "U.ARC")) continue;

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
        public static void LoadCollisionTiles()
        {
            // Get the assembly containing the embedded resource
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            // Get the stream of the embedded resource
            Stream resourceStream = assembly.GetManifestResourceStream("TeheManX4.Resources.MMX4B_Testtiles.bmp");

            //Get Data For Collision Textures
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.StreamSource = resourceStream;
            bmpImage.EndInit();
            collisionBmp = new WriteableBitmap(bmpImage);

        }
        public static unsafe void Draw16xTile(int id, int x, int y, int stride, IntPtr dest)
        {
            id &= 0x3FFF;
            byte* buffer = (byte*)dest;
            int val = BitConverter.ToInt32(PSX.levels[Id].tileInfo, id * 4);
            int page = (val >> 24) & 0xF;

            if (page > 0xB || id == 0) // 0 = Empty Tile
            {
                int index = x * 3 + y * stride;
                stride -= 48;
                for (int Y = 0; Y < 16; Y++)
                {
                    for (int X = 0; X < 16; X++)
                    {
                        *(ushort*)(buffer + index) = 0;
                        buffer[index + 2] = 0;
                        index += 3;
                    }
                    index += stride;
                }
                return;
            }

            // Get other Tile Info
            int cordX = (val >> 16) & 0xF;
            int cordY = (val >> 20) & 0xF;
            int clut = (val >> 8) & 0xFF;
            int collisionType = val & 0x3F;

            IntPtr bmpBackBuffer;
            int bmpStride;

            if(showCollision) //Collision
            {
                bmpStride = 128;
                bmpBackBuffer = collisionBmp.BackBuffer;

                for (int row = 0; row < 16; row++)
                {
                    int destIndex = (x * 3) + (y + row) * stride;
                    int sourceIndex = ((collisionType & 0xF) * 8) + (((collisionType >> 4) * 16 + row) * bmpStride);
                    for (int col = 0; col < 16; col++)
                    {
                        byte pixel;

                        pixel = *(byte*)(bmpBackBuffer + sourceIndex + (col / 2));
                        if ((col & 1) == 1)
                            pixel &= 0xF;
                        else
                            pixel >>= 4;

                        buffer[destIndex] = collisionBmp.Palette.Colors[pixel].R;
                        buffer[destIndex + 1] = collisionBmp.Palette.Colors[pixel].G;
                        buffer[destIndex + 2] = collisionBmp.Palette.Colors[pixel].B;
                        destIndex += 3;
                    }
                }
            }
            else if(page < 8) //4bpp
            {
                bmpBackBuffer = bmp[page + 8].BackBuffer;
                bmpStride = 128;

                for (int row = 0; row < 16; row++)
                {
                    int destIndex = (x * 3) + (y + row) * stride;
                    int sourceIndex = (cordX * 8) + ((cordY * 16 + row) * bmpStride);

                    for (int col = 0; col < 16; col++)
                    {
                        byte pixel;

                        pixel = *(byte*)(bmpBackBuffer + sourceIndex + (col / 2));
                        if ((col & 1) == 1)
                            pixel &= 0xF;
                        else
                            pixel >>= 4;

                        buffer[destIndex] = palette[clut + 64].Colors[pixel].R;
                        buffer[destIndex + 1] = palette[clut + 64].Colors[pixel].G;
                        buffer[destIndex + 2] = palette[clut + 64].Colors[pixel].B;
                        destIndex += 3;
                    }
                }
            }
            else //8bpp
            {
                bmpBackBuffer = bmp[(page & 3) + 16].BackBuffer;
                bmpStride = 256;

                for (int row = 0; row < 16; row++)
                {
                    int destIndex = (x * 3) + (y + row) * stride;
                    int sourceIndex = (cordX * 16) + ((cordY * 16 + row) * bmpStride);

                    for (int col = 0; col < 16; col++)
                    {
                        byte pixel;

                        pixel = *(byte*)(bmpBackBuffer + sourceIndex + col);
                        int indexClut = clut + (pixel >> 4);
                        pixel &= 0xF;
                        if ((indexClut * 16 + pixel) > 8191) continue;

                        buffer[destIndex] = palette[indexClut + 64].Colors[pixel].R;
                        buffer[destIndex + 1] = palette[indexClut + 64].Colors[pixel].G;
                        buffer[destIndex + 2] = palette[indexClut + 64].Colors[pixel].B;
                        destIndex += 3;
                    }
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
            this.clutAnime = this.arc.LoadEntry(13);
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
                uint p = BitConverter.ToUInt32(PSX.exe, Const.StartEnemyDataPointersOffset + i * 4);
                if (p != 0)
                    this.LoadEnemyData(PSX.CpuToOffset(p), true);

                if (enemyExpand)
                    this.LoadEnemyData();
                else
                {
                    p = BitConverter.ToUInt32(PSX.exe, Const.EnemyDataPointersOffset + i * 4);
                    if (p != 0)
                        this.LoadEnemyData(PSX.CpuToOffset(p));
                }
            }
        }
        private void ApplyLevelsToARC()
        {
            this.arc.SaveEntry(0, this.screenData);
            this.arc.SaveEntry(1, this.tileInfo);
            this.arc.SaveEntry(13, this.clutAnime);
            if (enemyExpand)
            {
                byte[] data = CreateEnemyData(this.enemies);
                Array.Resize(ref data,0x800);
                this.arc.SaveEntry(0x14, data);
            }
        }
        public unsafe void LoadTextures(bool onlyObj = false)
        {
            if (!onlyObj)
            {
                //BG Textures
                Array.Clear(pixels, 0, pixels.Length);
                this.arc.LoadEntry(0x010000, pixels);

                //8bpp
                for (int i = 0; i < 4; i++)
                {
                    bmp[16 + i].Lock();
                    byte* buffer = (byte*)bmp[16 + i].BackBuffer;
                    for (int r = 0; r < 256; r++)
                    {
                        for (int c = 0; c < 256; c++)
                        {
                            if(c < 128)
                                buffer[r * 256 + c] = pixels[i * 0x10000 + r * 128 + c];
                            else
                                buffer[r * 256 + c] = pixels[i * 0x10000 + 0x8000 + r * 128 + c - 128];
                        }
                    }

                    bmp[16 + i].AddDirtyRect(new Int32Rect(0, 0, 256, 256));
                    bmp[16 + i].Unlock();
                }

                //4bpp
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
                    int max = pixels.Length;
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (count == 0) //GOTO Y:0
                        {
                            addrW = i / 0x5800 * 0x8000;
                            count = 0x5800;
                        }
                        if (addrW >= max) break;
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
            int max = pixels.Length;
            for (int i = 0x2800; i < texData.Length; i++)
            {
                if (addrW >= max) break;
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
        public byte[] GetTriggerData()
        {
            int index = this.GetIndex();

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            for (int i = 0; i < Settings.MaxTriggers[index] + 1; i++)
            {
                uint addr = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + index * 4)));
                uint read = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(addr + i * 4)));

                bw.Write(BitConverter.ToUInt16(PSX.exe, PSX.CpuToOffset(read)));
                bw.Write(BitConverter.ToUInt16(PSX.exe, PSX.CpuToOffset(read + 2)));
                bw.Write(BitConverter.ToUInt16(PSX.exe, PSX.CpuToOffset(read + 4)));
                bw.Write(BitConverter.ToUInt16(PSX.exe, PSX.CpuToOffset(read + 6)));
                read += 8;

                while (true)
                {
                    ushort setting = BitConverter.ToUInt16(PSX.exe, PSX.CpuToOffset(read));
                    bw.Write(setting);
                    read += 2;
                    if (setting == 0)
                        break;
                }
            }
            return ms.ToArray();
        }
        private void LoadEnemyData()
        {
            try
            {
                byte[] data = this.arc.LoadEntry(0x14);
                int p = 0;
                while (true)
                {
                    if (data[p + 3] == 0xFF)
                        return;
                    //Add New Enemy
                    var e = new Enemy();
                    e.range = (byte)((data[p] >> 4) & 0xF);
                    e.id = data[p + 1];
                    e.var = data[p + 2];
                    e.type = data[p + 3];
                    e.x = BitConverter.ToInt16(data, p + 4);
                    e.y = BitConverter.ToInt16(data, p + 6);
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
                int enemiesCount = 0;
                
                int offset = 1;
                if (PSX.levels[id].arc.filename == "ST0B_0X.ARC")
                    offset = 2;

                if (enemyExpand)
                {
                    if (levelId != 9 && levelId != 0xA)
                        enemiesCount = PSX.levels[id].startEnemies.Count + PSX.levels[id + offset].startEnemies.Count;
                    else
                        enemiesCount = PSX.levels[id].startEnemies.Count;
                }
                else
                {
                    if (levelId != 9 && levelId != 0xA)
                        enemiesCount = PSX.levels[id].enemies.Count + PSX.levels[id].startEnemies.Count + PSX.levels[id + offset].enemies.Count + PSX.levels[id + offset].startEnemies.Count;
                    else
                        enemiesCount = PSX.levels[id].enemies.Count + PSX.levels[id].startEnemies.Count;
                }
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
                MessageBox.Show("The Reload Feature needs the player file in order for the feature to work.");
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
                else if(entry.type == 13)
                    Settings.levelClutAnimeAddress = (uint)(Settings.levelSize + Settings.levelStartAddress);
                else if(entry.type == 0x14)
                    Settings.levelEnemyAddress = (uint)(Settings.levelSize + Settings.levelStartAddress);
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
                int freeOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.EnemyDataPointersOffset + index * 4));
                byte[] data;
                //Regular Enemies
                if (!enemyExpand)
                {
                    data = CreateEnemyData(PSX.levels[id].enemies);
                    data.CopyTo(PSX.exe, freeOffset);
                    BitConverter.GetBytes(PSX.OffsetToCpu(freeOffset)).CopyTo(PSX.exe, Const.EnemyDataPointersOffset + index * 4);
                    freeOffset += data.Length;
                }

                //Start Enemies
                data = CreateEnemyData(PSX.levels[id].startEnemies);
                data.CopyTo(PSX.exe, freeOffset);
                BitConverter.GetBytes(PSX.OffsetToCpu(freeOffset)).CopyTo(PSX.exe, Const.StartEnemyDataPointersOffset + index * 4);
                freeOffset += data.Length;

                //For Mid Stage
                if (levelId != 0x9 && levelId != 0xA)
                {
                    int offset = 1;
                    if (PSX.levels[id].arc.filename == "ST0B_0X.ARC")
                        offset = 2;

                    if (!enemyExpand)
                    {
                        //Regular Enemies
                        data = CreateEnemyData(PSX.levels[id + offset].enemies);
                        data.CopyTo(PSX.exe, freeOffset);
                        BitConverter.GetBytes(PSX.OffsetToCpu(freeOffset)).CopyTo(PSX.exe, Const.EnemyDataPointersOffset + (index + 1) * 4);
                        freeOffset += data.Length;
                    }
                    //Start Enemies
                    data = CreateEnemyData(PSX.levels[id + offset].startEnemies);
                    data.CopyTo(PSX.exe, freeOffset);
                    BitConverter.GetBytes(PSX.OffsetToCpu(freeOffset)).CopyTo(PSX.exe, Const.StartEnemyDataPointersOffset + (index + 1) * 4);
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
            int length;
            if (!zeroFlag && PSX.levels[Id].clut_X != null)
                length = PSX.levels[Id].clut_X.entries[0].data.Length / 32;
            else if (zeroFlag && PSX.levels[Id].clut_Z != null)
                length = PSX.levels[Id].clut_Z.entries[0].data.Length / 32;
            else
                length = 0x80;
            if (length > palette.Length)
                length = palette.Length;



            for (int b = 0; b < length; b++)
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
                if(a < 16)
                {
                    if (bmp[a] == null)
                        bmp[a] = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Indexed4, Const.GreyScalePallete);
                }
                else
                {
                    if (bmp[a] == null)
                        bmp[a] = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Indexed8, Const.GreyScalePallete);
                }
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
        public static string CreateTilesetString(int baseX,int  baseY,int screenX,int screenY,int cols,int rows,byte layer,bool includeStruct,bool includeTable,string baseName,bool includeInfo)
        {
            System.Text.StringBuilder writter = new System.Text.StringBuilder();
            if (includeStruct)
                writter.AppendLine(Const.tileSetStruct);

            byte settings;
            if(cols > rows)
            {
                settings = (byte)((cols << 1) + 1);
                if (includeTable)
                {
                    writter.AppendLine($"/*{baseName} Tile Tables*/");
                    for (int y = 0; y < rows; y++)
                    {
                        writter.AppendLine("ushort " + baseName + "_tileTable" + y.ToString("X") + "[] = {");
                        for (int x = 0; x < cols; x++)
                        {
                            ushort id = BitConverter.ToUInt16(PSX.levels[Id].screenData, MainWindow.window.screenE.screenId * 0x200 + ((x + screenX) * 2) + (y + screenY) * 32);

                            if (x == (cols - 1))
                                writter.AppendLine("\t0x" + id.ToString("X"));
                            else
                                writter.AppendLine("\t0x" + id.ToString("X") + ",");
                        }
                        writter.AppendLine("};");
                    }
                    writter.AppendLine("\n\n");
                }
                if (includeInfo)
                {
                    writter.AppendLine($"/*{baseName} Tilesets*/");
                    for (int y = 0; y < rows; y++)
                    {
                        ushort destY = (ushort)(baseY + y * 0x10);
                        string boolStr;
                        if (y == (rows - 1))
                            boolStr = ",\n\tfalse";
                        else
                            boolStr = ",\n\ttrue";

                        writter.AppendLine($"tilesetStruct {baseName}_tileset" + y.ToString("X") + " = {");
                        writter.AppendLine("\t" + layer.ToString() +
                                            ",\n\t0x" + settings.ToString("X") +
                                            ",\n\t0x" + baseX.ToString("X") +
                                            ",\n\t0x" + destY.ToString("X") +
                                            ",\n\t0" +
                                            ",\n\t&" + baseName + "_tileTable" + y.ToString("X") +
                                            boolStr +
                                            "\n};");
                    }
                }
            }
            else
            {
                settings = (byte)(rows << 1); //Increament Y
                if (includeTable)
                {
                    writter.AppendLine($"/*{baseName} Tile Tables*/");
                    for (int x = 0; x < cols; x++)
                    {
                        writter.AppendLine($"ushort {baseName}_tileTable" + x.ToString("X") + "[] = {");
                        for (int y = 0; y < rows; y++)
                        {
                            ushort id = BitConverter.ToUInt16(PSX.levels[Id].screenData, MainWindow.window.screenE.screenId * 0x200 + ((x + screenX) * 2) + (y + screenY) * 32);

                            if (y == (rows - 1))
                                writter.AppendLine("\t0x" + id.ToString("X"));
                            else
                                writter.AppendLine("\t0x" + id.ToString("X") + ",");
                        }
                        writter.AppendLine("};");
                    }
                    writter.AppendLine("\n\n");
                }
                if (includeInfo)
                {
                    writter.AppendLine($"/*{baseName} Tilesets*/");
                    for (int x = 0; x < cols; x++)
                    {
                        ushort destX = (ushort)(baseX + x * 0x10);
                        string boolStr;
                        if (x == (cols - 1))
                            boolStr = ",\n\tfalse";
                        else
                            boolStr = ",\n\ttrue";

                        writter.AppendLine($"tilesetStruct {baseName}_tileset" + x.ToString("X") + " = {");
                        writter.AppendLine("\t" + layer.ToString() +
                                            ",\n\t0x" + settings.ToString("X") +
                                            ",\n\t0x" + destX.ToString("X") +
                                            ",\n\t0x" + baseY.ToString("X") +
                                            ",\n\t0" +
                                            ",\n\t&" + baseName + "_tileTable" + x.ToString("X") +
                                            boolStr +
                                            "\n};");
                    }
                }
            }
            return writter.ToString();
        }
        public static void ClearInvalidTiles()
        {
            //Clear Non Existing Tiles
            int maxTileId = PSX.levels[Id].tileInfo.Length / 4;
            maxTileId--;
            for (int s = 0; s < PSX.levels[Id].screenData.Length / 0x200; s++)
            {
                for (int t = 0; t < 0x100; t++)
                {
                    int index = s * 0x200 + t * 2;
                    ushort id = (ushort)(BitConverter.ToUInt16(PSX.levels[Id].screenData, index) & 0x3FFF);

                    if (id > maxTileId)
                    {
                        PSX.levels[Id].screenData[index] = 0;
                        PSX.levels[Id].screenData[index + 1] = 0;
                    }
                }
            }
        }
        public static void DecompressTexture(byte[] src, byte[] dest, int start = 0)
        {
            int offset = start;
            int destOffset = 0;

        Start:  //Get New CONTROL Byte
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
                        data >>= 0xB;   //Length
                        data2 &= 0x7FF; //Copy Offset
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
