using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using TeheManX4.Forms;

namespace TeheManX4
{
    static class PSX
    {
        #region Fields
        static string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
        static Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
        static byte[] sector = new byte[0x800];
        static public List<Level> levels = new List<Level>();
        static public byte[] exe;
        static public bool edit = false;
        static public string filePath;
        static public string lastSave;
        static public DateTime time; //for EXE
        static BinaryWriter bw;
        static BinaryReader br;
        static FileStream fs;
        #endregion Fields

        #region Methods
        public static void OpenFileBrowser(string path)
        {
            try
            {
                fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                br = new BinaryReader(fs);
                bw = new BinaryWriter(fs);
                //Check to See if this is ISO 9660
                br.BaseStream.Position = 0x18 + (0x930 * 16);
                sector = br.ReadBytes(sector.Length);
                if (System.Text.Encoding.ASCII.GetString(sector, 1, 5) != "CD001")
                {
                    System.Windows.MessageBox.Show("Not a valid ISO 9660 Image");
                    return;
                }

                List<WriteFile> rootFolders = new List<WriteFile>();
                //Get LBA Volume Location
                uint lba = BitConverter.ToUInt32(sector, 140);
                br.BaseStream.Position = 0x930 * lba;
                sector = br.ReadBytes(sector.Length);

                br.BaseStream.Position = BitConverter.ToUInt32(sector, 0x1A) * 0x930;

                //Make WINDOW + CONTROLS
                ListWindow win = new ListWindow();

                //Use Title to save Path
                win.Title = path;
                win.MaxWidth = win.Width;
                win.ResizeMode = System.Windows.ResizeMode.CanMinimize;
                win.mode = 0xFF;
                //Read Main Directory
                rootFolders = GetDirectory(BitConverter.ToUInt32(sector, 0x1A));
                Button home = new Button();
                home.Content = "<GOTO ROOT>";
                home.Height = 40;
                home.Uid = BitConverter.ToUInt32(sector, 0x1A).ToString();
                home.Click += (s, e) =>
                {
                    List<WriteFile> list = GetDirectory(Convert.ToUInt32(home.Uid));
                    while (true)
                    {
                        if (win.grid.RowDefinitions.Count >= list.Count)
                            break;
                        win.grid.RowDefinitions.Add(new RowDefinition());
                    }
                    List<Button> listB = new List<Button>();

                    for (int i = 0; i < win.grid.Children.Count; i++)
                    {
                        if (win.grid.Children[i].GetType() != typeof(Button))
                            continue;
                        listB.Add(win.grid.Children[i] as Button);
                    }
                    for (int i = 0; i < listB.Count; i++)
                    {
                        win.grid.Children.Remove(listB[i]);
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        Button b = new Button();
                        b.Content = list[i].name;
                        b.Uid = list[i].lba.ToString() + " " + list[i].offset + " " + list[i].size;
                        b.Click += B_Click;
                        b.Tag = win;
                        Grid.SetRow(b, i);
                        win.grid.Children.Add(b);
                    }
                };
                Grid.SetRow(home, 1);

                while (true)
                {
                    if (win.grid.RowDefinitions.Count == rootFolders.Count)
                        break;
                    win.grid.RowDefinitions.Add(new RowDefinition());
                }
                for (int i = 0; i < rootFolders.Count; i++)
                {
                    Button b = new Button();
                    b.Content = rootFolders[i].name;
                    b.Uid = rootFolders[i].lba.ToString() + " " + rootFolders[i].offset + " " + rootFolders[i].size;
                    b.Click += B_Click;
                    b.Tag = win;
                    Grid.SetRow(b, i);
                    win.grid.Children.Add(b);
                }
                win.outGrid.RowDefinitions.Add(new RowDefinition());
                win.outGrid.RowDefinitions.Add(new RowDefinition());
                win.outGrid.RowDefinitions[1].Height = System.Windows.GridLength.Auto;
                win.outGrid.Children.Add(home);
                win.ShowDialog();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "ERROR");
                System.Windows.Application.Current.Shutdown();
            }
        }
        public static void Extract(string path, string save)
        {
            try
            {
                fs = File.OpenRead(path);
                br = new BinaryReader(fs);
                //Check to See if this is ISO 9660
                br.BaseStream.Position = 0x18 + (0x930 * 16);
                sector = br.ReadBytes(sector.Length);
                if (System.Text.Encoding.ASCII.GetString(sector, 1, 5) != "CD001")
                {
                    System.Windows.MessageBox.Show("Not a valid ISO 9660 Image");
                    fs.Dispose();
                    br.Dispose();
                    return;
                }

                List<WriteFile> directory = new List<WriteFile>();
                //Get LBA Volume Location
                uint lba = BitConverter.ToUInt32(sector, 140);
                br.BaseStream.Position = 0x930 * lba;
                sector = br.ReadBytes(sector.Length);

                //Read Main Directory
                directory = GetDirectory(BitConverter.ToUInt32(sector, 0x1A));
                List<WriteFile> extraDirectory = new List<WriteFile>();
            DirectoryLoop:
                foreach (var f in directory)
                {
                    if (f.isFolder)
                    {
                        WriteFile w = new WriteFile();
                        w.lba = f.lba;
                        w.name = f.name;
                        w.path = save + "/" + f.name;
                        w.size = f.size;
                        w.isFolder = f.isFolder;
                        extraDirectory.Add(w);
                    }
                    else
                        ExtractFile(f.lba, f.size, save, f.name);
                }
                directory.Clear();
                if (extraDirectory.Count != 0)
                {
                    directory = GetDirectory(extraDirectory[0].lba);
                    save = extraDirectory[0].path;
                    extraDirectory.RemoveAt(0);
                    Directory.CreateDirectory(save);
                    goto DirectoryLoop;
                }
                br.Dispose();
                fs.Dispose();
                System.Windows.MessageBox.Show("Extracted !");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }
        private static List<WriteFile> GetDirectory(uint lba)
        {
            br.BaseStream.Position = lba * 0x930;
            sector = br.ReadBytes(0x930);
            int offset = sector[0x18] + 0x18;
            offset += sector[offset];

            List<WriteFile> l = new List<WriteFile>();

            while (true)
            {
                WriteFile w = new WriteFile();
                w.name = System.Text.Encoding.ASCII.GetString(sector, 33 + offset, sector[32 + offset]);

                if (!w.name.Contains(";1"))
                    w.isFolder = true;
                else
                {

                }
                w.lba = BitConverter.ToUInt32(sector, offset + 2);
                w.size = BitConverter.ToInt32(sector, offset + 10);
                w.offset = offset + (br.BaseStream.Position - 0x930);
                l.Add(w);

                offset += sector[offset];

                if (sector[offset] < 0x30 || offset > 0x817)
                {
                    if ((sector[0x12] & 0x1) == 1)
                    {
                        break;
                    }
                    //Move to next sector
                    sector = br.ReadBytes(0x930);
                    offset = sector[0x18] + 0x18;
                    offset += sector[offset];
                }
            }
            return l;
        }
        public static void SerialHalt(bool w = false)
        {
            Settings.nops.StartInfo.Arguments = "/halt COM" + MainWindow.settings.comPort;
            Settings.nops.Start();
            if (w)
                Settings.nops.WaitForExit();
        }
        public static void SerialCont()
        {
            Settings.nops.StartInfo.Arguments = "/cont COM" + MainWindow.settings.comPort;
            Settings.nops.Start();
            Settings.nops.WaitForExit();
        }
        public static void SerialWrite(uint addr, int v, bool w = false)
        {
            Settings.nops.StartInfo.Arguments = "/poke32 0x" + Convert.ToString(addr, 16) + " 0x" + Convert.ToString(v, 16) + " COM" + MainWindow.settings.comPort;
            Settings.nops.Start();
            if (w)
                Settings.nops.WaitForExit();
        }
        public static void SerialWrite(uint addr, ushort v, bool w = false)
        {
            Settings.nops.StartInfo.Arguments = "/poke16 0x" + Convert.ToString(addr, 16) + " 0x" + Convert.ToString(v, 16) + " COM" + MainWindow.settings.comPort;
            Settings.nops.Start();
            if (w)
                Settings.nops.WaitForExit();
        }
        public static void SerialWrite(uint addr, byte v, bool w = false)
        {
            Settings.nops.StartInfo.Arguments = "/poke8 0x" + Convert.ToString(addr, 16) + " 0x" + Convert.ToString(v, 16) + " COM" + MainWindow.settings.comPort;
            Settings.nops.Start();
            if (w)
                Settings.nops.WaitForExit();
        }
        public static void SerialWrite(uint addr, byte[] vals, bool w = false)
        {
            File.WriteAllBytes(Const.CachName, vals);
            Settings.nops.StartInfo.Arguments = "/bin 0x" + Convert.ToString(addr, 16) + " " + Const.CachName + " COM" + MainWindow.settings.comPort;
            Settings.nops.Start();
            if (w)
                Settings.nops.WaitForExit();
        }
        private static void ExtractFile(uint lba, int fileSize, string save, string filename)
        {
            byte[] data;
            int o = 0;
            filename = filename.Replace(";1", "");
            if (!filename.EndsWith(".STR") && !filename.EndsWith(".XA"))
                data = new byte[0x800];
            else if (filename.EndsWith(".DA"))
            {
                data = new byte[0x930];
                o = 0x18;
            }
            else
            {
                data = new byte[0x920];
                o = 8;
            }

            var bw = new BinaryWriter(File.Create(save + "/" + filename));
            br.BaseStream.Position = GetSectorOffset(lba) - o;
            while (fileSize != 0)
            {
                if (filename.EndsWith(".DA"))
                {
                    if (fileSize >= 0x800)
                    {
                        data = br.ReadBytes(0x930);
                        bw.Write(data);
                        fileSize -= 0x800;
                        if (fileSize <= 0)
                            break;
                    }
                    else
                    {
                        data = br.ReadBytes(fileSize);
                        bw.Write(data, 0, fileSize);
                        fileSize = 0;
                    }
                }
                else if (!filename.EndsWith(".STR") && !filename.EndsWith(".XA"))
                {
                    if (fileSize > 0x800)
                    {
                        data = br.ReadBytes(0x800);
                        bw.Write(data);
                        br.BaseStream.Position += 0x130;
                        fileSize -= 0x800;
                        if (fileSize == 0)
                            break;
                    }
                    else
                    {
                        data = br.ReadBytes(fileSize);
                        bw.Write(data, 0, fileSize);
                        fileSize = 0;
                    }
                }
                else
                {
                    if (true)
                    {
                        data = br.ReadBytes(0x920);
                        bw.Write(data);
                        br.BaseStream.Position += 0x10;
                        fileSize -= 0x800;
                    }
                }
            }
            bw.Close();
        }
        private static int GetMovieNormalFileSize(int fileSize)
        {
            int size = 0;
            if (fileSize % 0x920 != 0)
                size++;
            size += fileSize / 0x920;
            return fileSize - size * 0x120;
        }
        private static byte[] CdIntToPos(int i) //Absolute Sector => Time Code
        {
            int v3 = (i + 0x96) / 0x4B;
            int v2 = (i + 0x96) % 0x4B;
            int v1 = v3 / 0x3C;
            v3 = v3 % 0x3C;

            var time = new byte[3];

            time[1] = (byte)(v3 + (v3 / 10) * 6);
            time[2] = (byte)(v2 + (v2 / 10) * 6);
            time[0] = (byte)(v1 + (v1 / 10) * 6);
            return time;
        }
        public static int CdPosToInt(byte[] i) //Time Code => Absolute Sector
        {
            return (((i[0] >> 4) * 10 + (i[0] & 0xF)) * 0x3C + (i[1] >> 4) * 10 + (i[1] & 0xF)) * 0x4B + (i[2] >> 4) * 10 + (i[2] & 0xF) + -150;
        }
        private static uint ComputeEDC(byte[] src, int size) //Pass whole Sector & 0x808
        {
            uint edc = 0;
            int offset = 0;
            for (; size != 0; size--)
            {
                edc = (edc >> 8) ^ Const.c_edc_lut[(edc ^ src[offset + 0x10]) & 0xFF];
                offset++;
            }
            return edc;
        }
        private static void EccWritePQ(byte[] src, uint major_count, uint minor_count, uint major_mult, uint minor_inc, int offset = 0)
        {
            uint size = major_count * minor_count;
            uint major, minor;
            for (major = 0; major < major_count; major++)
            {
                uint index = (major >> 1) * major_mult + (major & 1);
                uint ecc_a = 0;
                uint ecc_b = 0;
                for (minor = 0; minor < minor_count; minor++)
                {
                    uint temp = src[index + 0xC];
                    index += minor_inc;
                    if (index >= size) index -= size;
                    ecc_a ^= temp;
                    ecc_b ^= temp;
                    ecc_a = Const.c_ecc_f_lut[ecc_a];
                }
                ecc_a = Const.c_ecc_b_lut[Const.c_ecc_f_lut[ecc_a] ^ ecc_b];
                src[major + offset] = (byte)ecc_a;
                src[major + major_count + offset] = (byte)(ecc_a ^ ecc_b);
            }
        }
        public static void ComputeEcc(byte[] data, int offset = 0)
        {
            //Save Header
            byte[] head = new byte[4];
            Array.Copy(data, 12 + offset, head, 0, 4);
            Array.Clear(data, 12 + offset, 4);

            //Compute P Code
            EccWritePQ(data, 86, 24, 2, 86, 0x81C + offset);
            //Compute Q Code
            EccWritePQ(data, 52, 43, 86, 88, 0x8C8 + offset);


            //Restore Header
            Array.Copy(head, 0, data, 12, 4);
        }
        public static int CpuToOffset(uint cpu)
        {
            return (int)(cpu - BitConverter.ToUInt32(exe, 0x18) + 0x800);
        }
        public static int CpuToOffset(uint cpu, uint text)
        {
            return (int)(cpu - text + 0x800);
        }
        public static uint OffsetToCpu(int offset)
        {
            return (uint)(offset + BitConverter.ToUInt32(exe, 0x18) - 0x800);
        }
        private static long GetSectorOffset(uint lba)
        {
            return 0x18 + (lba * 0x930);
        }
        private static void ReadSector()
        {
            sector = br.ReadBytes(sector.Length);
        }
        private static void ClearSector()
        {
            for (int i = 0; i < sector.Length; i++)
            {
                sector[i] = 0;
            }
        }
        #endregion Methods

        #region Events
        private static void B_Click(object sender, System.Windows.RoutedEventArgs e) //For File Browser
        {
            ListWindow win = ((Button)sender).Tag as ListWindow;
            if(fs == null)
            {
                try
                {
                    fs = File.Open(win.Title, FileMode.Open, FileAccess.ReadWrite);
                    br = new BinaryReader(fs);
                    bw = new BinaryWriter(fs);
                }
                catch(Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return;
                }
            }
            if (((Button)sender).Content.ToString().Contains(";1"))
            {
                try
                {
                    using (var fd = new System.Windows.Forms.OpenFileDialog())
                    {
                        fd.Title = "Replace - " + ((Button)sender).Content.ToString();
                        if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            //DA files not supported (for now atleast)
                            if (fd.FileName.EndsWith(".DA"))
                            {
                                System.Windows.MessageBox.Show("DA Files are not supported");
                                return;
                            }
                            //Give WARNING if file is bigger than file being replaced
                            FileInfo fileInfo = new FileInfo(fd.FileName);
                            int actualSize;
                            if (fd.FileName.Contains(".STR") || fd.FileName.Contains(".XA"))
                                actualSize = GetMovieNormalFileSize((int)fileInfo.Length);
                            else
                                actualSize = (int)fileInfo.Length;

                            if(actualSize > Convert.ToInt32(((Button)sender).Uid.Split()[2]))
                            {
                                var result = System.Windows.MessageBox.Show("This file is bigger than the file that is being replace.\nDo you still want to replace it?", "WARNING", System.Windows.MessageBoxButton.OKCancel);
                                if (result != System.Windows.MessageBoxResult.OK)
                                    return;
                            }


                            sector = new byte[0x930];
                            byte[] data = File.ReadAllBytes(fd.FileName);
                            BinaryReader br2 = new BinaryReader(new MemoryStream(data));
                            int size = data.Length;

                            bw.BaseStream.Position = Convert.ToInt64(((Button)sender).Uid.Split()[0]) * 0x930;

                            bool xa = false;
                            if (fd.FileName.Replace(";1", "").EndsWith(".STR") || fd.FileName.Replace(";1", "").EndsWith(".XA"))
                                xa = true;

                            while (size != 0)
                            {
                                ClearSector();
                                Const.Sync.CopyTo(sector, 0);
                                CdIntToPos((int)(bw.BaseStream.Position / 0x930)).CopyTo(sector, 0xC);
                                sector[0xF] = 2;


                                if (!xa) //Normal File
                                {
                                    if (size > 0x800)
                                    {
                                        sector[0x12] = 8;
                                        sector[0x16] = 8;
                                        size -= 0x800;
                                        br2.ReadBytes(0x800).CopyTo(sector, 0x18);
                                    }
                                    else
                                    {
                                        sector[0x12] = 0x89;
                                        sector[0x16] = 0x89;
                                        br2.ReadBytes(size).CopyTo(sector, 0x18);
                                        size = 0;

                                        if(data.Length != Convert.ToInt64(((Button)sender).Uid.Split()[2]))
                                        {
                                            //TODO: update EDC/ECC for directory data
                                            long backup = bw.BaseStream.Position;
                                            bw.BaseStream.Position = Convert.ToInt64(((Button)sender).Uid.Split()[1]) + 10;
                                            byte[] sz = BitConverter.GetBytes(data.Length);
                                            bw.Write(sz);
                                            Array.Reverse(sz);
                                            bw.Write(sz);

                                            //Fix EDC & ECC
                                            br.BaseStream.Position = Convert.ToInt64(((Button)sender).Uid.Split()[1]) / 0x930;
                                            ReadSector();
                                            ComputeEDC(sector, 0x808);
                                            ComputeEcc(sector);

                                            bw.BaseStream.Position = backup;
                                        }
                                    }
                                    BitConverter.GetBytes(ComputeEDC(sector, 0x808)).CopyTo(sector, 0x818);
                                    ComputeEcc(sector);
                                    bw.Write(sector);
                                }
                                else //STR & XA
                                {
                                    if (size > 0x920)
                                    {
                                        size -= 0x920;
                                        br2.ReadBytes(0x920).CopyTo(sector, 0x10);
                                    }
                                    else
                                    {
                                        br2.ReadBytes(size).CopyTo(sector, 0x10);
                                        size = 0;

                                        if (data.Length != Convert.ToInt64(((Button)sender).Uid.Split()[2])) //Update File Length
                                        {
                                            long backup = bw.BaseStream.Position;
                                            bw.BaseStream.Position = Convert.ToInt64(((Button)sender).Uid.Split()[1]) + 10;
                                            byte[] sz = BitConverter.GetBytes(GetMovieNormalFileSize(data.Length));
                                            bw.Write(sz);
                                            Array.Reverse(sz);
                                            bw.Write(sz);

                                            //Fix EDC & ECC
                                            br.BaseStream.Position = Convert.ToInt64(((Button)sender).Uid.Split()[1]) / 0x930;
                                            ReadSector();
                                            ComputeEDC(sector, 0x808);
                                            ComputeEcc(sector);

                                            bw.BaseStream.Position = backup;
                                        }
                                    }
                                    bw.Write(sector);
                                }
                            }
                            br.Close();
                            bw.Close();
                            fs.Close();
                            fs = null;
                            System.Windows.MessageBox.Show("File Replaced!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    System.Windows.Application.Current.Shutdown();
                }
            }
            else
            {
                List<WriteFile> list = GetDirectory(Convert.ToUInt32(((Button)sender).Uid.Split()[0]));
                while (true)
                {
                    if (win.grid.RowDefinitions.Count >= list.Count)
                        break;
                    win.grid.RowDefinitions.Add(new RowDefinition());
                }
                List<Button> listB = new List<Button>();

                for (int i = 0; i < win.grid.Children.Count; i++)
                {
                    if (win.grid.Children[i].GetType() != typeof(Button))
                        continue;
                    listB.Add(win.grid.Children[i] as Button);
                }
                for (int i = 0; i < listB.Count; i++)
                {
                    win.grid.Children.Remove(listB[i]);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    //TODO: make these some sort of labels (in ListView) instead & put button on bottom
                    Button b = new Button();
                    b.Content = list[i].name;
                    b.Uid = list[i].lba.ToString() + " " + list[i].offset + " " + list[i].size;
                    b.Click += B_Click;
                    b.Tag = win;
                    Grid.SetRow(b, i);
                    win.grid.Children.Add(b);
                }
            }
        }
        #endregion Events
    }
}
